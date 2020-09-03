import React, { createContext, useReducer, useEffect } from "react"
import { ClientHub, CONNECTION_STATE } from "../services/hub"
import { handleLogin, handleRegister } from "../services/auth"

const MESSAGE_TYPE = {
  USER_INFO: 0,
  GAME_UPDATE: 1,
  END_GAME: 2,
}

const ACTION_TYPE = {
  USER_INFO: 0,
  GAME_UPDATE: 1,
  END_GAME: 2,

  REGISTER: 3,
  LOGIN: 4,
}

export const GAME_STATE = {
  NONE: -1,
  WAITING: 0,
  START: 1,
  COMPARE: 2,
  END: 3,
}

export const HAND = {
  DEFAULT: 0,
  ROCK: 1,
  PAPER: 2,
  SCISSORS: 3,
}

const initialState = {
  auth: {
    isAuthenticated: false,
    userAuth: null,
  },
  game: {
    gameId: "",
    round: 0,
    gameState: GAME_STATE.NONE,
    you: null,
    opponent: null,
  },
}
export const AppContext = createContext(initialState)

const reducer = (state, action) => {
  switch (action.type) {
    case ACTION_TYPE.USER_INFO:
      return {
        ...state,
      }
    case ACTION_TYPE.GAME_UPDATE:
      console.log("action.payload.data", action.payload.data)
      return {
        ...state,
        game: {
          gameId: action.payload.data.gameId,
          round: action.payload.data.round,
          gameState: action.payload.data.state,
          you: action.payload.data.playersState.find(
            e => e.name === state.auth.userAuth.username
          ),
          opponent:
            action.payload.data.playersState.length > 1
              ? action.payload.data.playersState.find(
                  e => e.name !== state.auth.userAuth.username
                )
              : null,
        },
      }
    case ACTION_TYPE.END_GAME:
      return { ...state, game: initialState.game }

    case ACTION_TYPE.LOGIN:
      return {
        ...state,
        auth: {
          isAuthenticated: true,
          userAuth: action.payload.data,
        },
      }
    case ACTION_TYPE.LOGOUT:
      return initialState
    default:
  }
}
const clientHub = new ClientHub()

export default function AppContextProvider({ children }) {
  const [state, dispatch] = useReducer(reducer, initialState)

  const messageHandler = msg => {
    const data = msg.data
    switch (msg.messageType) {
      case MESSAGE_TYPE.USER_INFO:
        dispatch({
          type: ACTION_TYPE.USER_INFO,
          payload: {
            data: data,
          },
        })
        break
      case MESSAGE_TYPE.GAME_UPDATE:
        dispatch({
          type: ACTION_TYPE.GAME_UPDATE,
          payload: {
            data: data,
          },
        })
        break
      default:
    }
  }

  useEffect(() => {
    if (
      clientHub.ConnectionState === CONNECTION_STATE.NONE &&
      state.auth.isAuthenticated
    ) {
      clientHub.initialize(state.auth.userAuth.accessToken)
    }
    if (
      state.auth.isAuthenticated &&
      clientHub.ConnectionState === CONNECTION_STATE.INITIALIZE
    ) {
      if (clientHub === null) clientHub = new ClientHub()
      clientHub
        .start()
        .then(() => {
          clientHub.messageHandler = messageHandler
        })
        .catch(err => {
          console.error(err)
        })
    }
    if (
      !state.auth.isAuthenticated &&
      clientHub.ConnectionState === CONNECTION_STATE.CONNECTED
    ) {
      clientHub.stop()
    }
  }, [state.auth.isAuthenticated])

  const createGame = () => {
    clientHub.invoke("CreateGame")
  }

  const joinGame = gameId => {
    // clientHub.invoke("JoinGame", gameId)
    clientHub.invoke("JoinWaitingGame")
  }

  const playHand = hand => {
    clientHub.invoke("PlayHand", state.game.gameId, hand)
  }

  const leftGame = gameId => {
    clientHub.invoke("LeftGame", state.game.gameId)
    dispatch({
      type: ACTION_TYPE.END_GAME,
      payload: {},
    })
  }

  const register = (name, email, password) => {
    return handleRegister(name, email, password)
      .then(userAuth => {
        dispatch({
          type: ACTION_TYPE.LOGIN,
          payload: {
            data: userAuth,
          },
        })
      })
      .catch(err => {
        throw err
      })
  }

  const login = (name, password) => {
    return handleLogin(name, password)
      .then(userAuth => {
        dispatch({
          type: ACTION_TYPE.LOGIN,
          payload: {
            data: userAuth,
          },
        })
      })
      .catch(err => {
        throw err
      })
  }

  const logout = () => {
    state.auth.userAuth.logout()
    dispatch({
      type: ACTION_TYPE.LOGOUT,
      payload: {},
    })
  }

  const value = {
    state,
    createGame,
    joinGame,
    playHand,
    leftGame,
    login,
    register,
    logout,
  }

  return <AppContext.Provider value={value}>{children}</AppContext.Provider>
}
