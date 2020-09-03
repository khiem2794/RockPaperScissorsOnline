import React, { useContext } from "react"
import Layout from "../components/layout"
import { AppContext } from "../context/context"

export default function Index() {
  const ctx = useContext(AppContext)
  const create = () => {
    console.log(ctx)
  }
  return (
    <Layout>
      <div>Index page</div>
      <button onClick={() => create()}>Create</button>
    </Layout>
  )
}
