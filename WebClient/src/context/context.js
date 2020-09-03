import React, { createContext, useReducer, useEffect, useRef } from "react"
import { ClientHub } from "../signalr/hub"

const MESSAGE_TYPE = {
  USER_INFO: 0,
  GAME_UPDATE: 1,
}

const Hand = {
  ROCK: 1,
  PAPER: 2,
  SCISSORS: 3,
}

const initialState = {}
export const AppContext = createContext(initialState)

const reducer = (state, action) => {
  switch (action.type) {
    default:
  }
}
const clientHub = new ClientHub()
export default function AppContextProvider({ children }) {
  const [state, dispatch] = useReducer(reducer, initialState)

  const messageHandler = msg => {
    console.log(msg)
    const data = msg.data
    switch (msg.messageType) {
      case MESSAGE_TYPE.USER_INFO:
        this.connectionId = data.connectionId
        this.userName = data.userName
        break
      case MESSAGE_TYPE.GAME_UPDATE:
        this.game = data
        break
      default:
    }
  }

  useEffect(() => {
    console.log("sad")
    clientHub
      .start()
      .then(() => {
        clientHub.messageHandler = messageHandler
      })
      .catch(err => {
        console.error(err)
      })
    return () => {
      console.log("stop")
      clientHub.stop()
    }
  }, [])

  const createGame = () => {
    clientHub.invoke("CreateGame")
  }
  // const joinGame = gameId => {
  //   clientHub.invoke("JoinGame", gameId)
  // }
  // const playHand = hand => {
  //   clientHub.invoke("PlayHand", this.game.gameId, hand)
  // }

  const playHand = hand => {
    dispatch({
      type: "",
      payload: {
        data: null,
      },
    })
  }

  const value = { state, createGame }
  return <AppContext.Provider value={value}>{children}</AppContext.Provider>
}
