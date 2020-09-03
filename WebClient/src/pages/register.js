import React, { useContext } from "react"
import { Grid, Card, makeStyles } from "@material-ui/core"
import Layout from "../components/layout"
import RegisterForm from "../components/register-form"
import { AppContext } from "../context/context"
import { navigate } from "gatsby"

const useStyles = makeStyles(theme => ({
  card: {
    padding: "25px",
    marginTop: "50px",
  },
}))

export default function Register() {
  const classes = useStyles()
  const { state, register } = useContext(AppContext)
  if (state.auth.isAuthenticated) {
    navigate("/")
    return <Layout>Already LoggedIn</Layout>
  }
  return (
    <Layout>
      <Grid container justify="center">
        <Grid item xs={10} md={8}>
          <Card className={classes.card} elevation={5}>
            <RegisterForm handleRegister={register} />
          </Card>
        </Grid>
      </Grid>
    </Layout>
  )
}
