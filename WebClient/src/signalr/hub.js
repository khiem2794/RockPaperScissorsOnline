import * as signalR from "@microsoft/signalr"

const CONNECTION_STATE = {
  INITIALIZE: 0,
  CONNECTED: 1,
  DISCONNECTED: 2,
}

const MESSAGE_TYPE = {
  USER_INFO: 0,
  GAME_UPDATE: 1,
}

const Hand = {
  ROCK: 1,
  PAPER: 2,
  SCISSORS: 3,
}

export class ClientHub {
  constructor() {
    this.ConnectionState = CONNECTION_STATE.INITIALIZE
    this.conn = new signalR.HubConnectionBuilder()
      .withUrl("https://localhost:5001/gamehub")
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build()
    this.handleMessage = null
    this.conn.onreconnected(() => {
      this.ConnectionState = CONNECTION_STATE.CONNECTED
    })
    this.conn.onclose(() => {
      this.ConnectionState = CONNECTION_STATE.CONNECTED
    })
    this.conn.on("MessageClient", msg => {
      if (this.handleMessage !== null) this.handleMessage(msg)
    })
  }
  start() {
    this.conn
      .start()
      .then(() => {
        this.ConnectionState = CONNECTION_STATE.CONNECTED
      })
      .catch(err => {
        this.ConnectionState = CONNECTION_STATE.DISCONNECTED
        throw err
      })
  }
  invoke(method, ...params) {
    if (this.ConnectionState === CONNECTION_STATE.CONNECTED)
      this.conn.invoke(method, ...params).catch(err => {
        throw err
      })
  }
}

export class User {
  constructor(clientHub) {
    this.clientHub = clientHub
    this.userName = ""
    this.connectionId = ""
    clientHub.handleMessage = msg => this.handleMessage(msg)
    this.game = null
  }
  handleMessage(msg) {
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
  createGame() {
    this.clientHub.invoke("CreateGame")
  }
  joinGame(gameId) {
    this.clientHub.invoke("JoinGame", gameId)
  }
  playHand(hand) {
    this.clientHub.invoke("PlayHand", this.game.gameId, hand)
  }
}
