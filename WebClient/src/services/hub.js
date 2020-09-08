import * as signalR from "@microsoft/signalr"
import ApiResources from "./api-resources"

export const CONNECTION_STATE = {
  NONE: -1,
  INITIALIZE: 0,
  CONNECTED: 1,
  DISCONNECTED: 2,
}

export class ClientHub {
  constructor() {
    this.ConnectionState = CONNECTION_STATE.NONE
  }
  initialize(userAuth) {
    this.ConnectionState = CONNECTION_STATE.INITIALIZE
    this.conn = new signalR.HubConnectionBuilder()
      .withUrl(ApiResources.gamehub, {
        accessTokenFactory: () => userAuth.accessToken,
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build()
    this.conn.messageHandler = null
    this.conn.onreconnected(() => {
      this.ConnectionState = CONNECTION_STATE.CONNECTED
    })
    this.conn.onclose(() => {
      this.ConnectionState = CONNECTION_STATE.NONE
    })
    this.conn.on("MessageClient", msg => {
      if (this.messageHandler !== null) this.messageHandler(msg)
    })
  }
  start() {
    return this.conn
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
  stop() {
    this.conn.stop()
  }
}
