import React from "react"
import Layout from "../components/layout"
import { ClientHub, User } from "../signalr/hub"

const hub = new ClientHub()
const user = new User(hub)

export default function Index() {
  const connect = () => {
    hub.start()
  }
  const create = () => {
    user.createGame()
  }
  return (
    <Layout>
      <div>Index page</div>
      <button onClick={() => connect()}>Connect</button>
      <button onClick={() => create()}>Create</button>
    </Layout>
  )
}
