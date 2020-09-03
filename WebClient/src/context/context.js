import React, { createContext, useReducer, useEffect } from "react"
import { ClientHub } from "../signalr/hub"

const MESSAGE_TYPE = {
  USER_INFO: 0,
  GAME_UPDATE: 1,
  END_GAME: 2,
}

const ACTION_TYPE = {
  USER_INFO: 0,
  GAME_UPDATE: 1,
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
  user: {
    userName: "",
    connectionId: "",
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
        user: {
          userName: action.payload.data.user.userName,
          connectionId: action.payload.data.user.connectionId,
        },
      }
    case ACTION_TYPE.GAME_UPDATE:
      return {
        ...state,
        game: {
          gameId: action.payload.data.gameId,
          round: action.payload.data.round,
          gameState: action.payload.data.state,
          you: action.payload.data.playersState.find(
            e => e.name === state.user.userName
          ),
          opponent:
            action.payload.data.playersState.length > 1
              ? action.payload.data.playersState.find(
                  e => e.name !== state.user.userName
                )
              : null,
        },
      }
    case ACTION_TYPE.END_GAME:
      return { ...state, game: initialState.game }
    default:
  }
}
const clientHub = new ClientHub()
export default function AppContextProvider({ children }) {
  const [state, dispatch] = useReducer(reducer, initialState)
  console.log("game", state.game)

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
    clientHub
      .start()
      .then(() => {
        clientHub.messageHandler = messageHandler
      })
      .catch(err => {
        console.error(err)
      })
  }, [])

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

  const value = { state, createGame, joinGame, playHand, leftGame }

  return <AppContext.Provider value={value}>{children}</AppContext.Provider>
}
