import React, { useState } from "react"
import { Grid, Button, TextField, Typography } from "@material-ui/core"
import { Link } from "gatsby"

export default function LoginForm({ handleLogin }) {
  const [userName, setUserName] = useState("")
  const [password, setPassword] = useState("")
  const [isLoggingIn, setIsLogginIn] = useState(false)
  const submitLogin = e => {
    e.preventDefault()
    setIsLogginIn(true)
    try {
      handleLogin(userName, password)
    } catch (error) {
      setIsLogginIn(false)
    }
  }
  return (
    <Grid item xs={12}>
      <form autoComplete="off" onSubmit={e => submitLogin(e)}>
        <Grid container justify="center">
          <Typography component="h1" variant="h5">
            Sign in
          </Typography>
          <Grid item xs={12}>
            <TextField
              type="text"
              name="username"
              label="User name"
              fullWidth
              margin="normal"
              InputLabelProps={{
                shrink: true,
              }}
              variant="outlined"
              onChange={e => setUserName(e.target.value)}
            />
          </Grid>
          <Grid item xs={12}>
            <TextField
              type="password"
              name="password"
              label="Password"
              fullWidth
              margin="normal"
              InputLabelProps={{
                shrink: true,
              }}
              variant="outlined"
              onChange={e => setPassword(e.target.value)}
            />
          </Grid>
          <Grid container justify="flex-end">
            <Button
              variant="contained"
              size="large"
              color="primary"
              type="submit"
              fullWidth
              disabled={isLoggingIn}
            >
              LOGIN
            </Button>
          </Grid>
          <Grid container style={{ marginTop: "15px" }}>
            <Grid item xs>
              <Link to="/login" variant="body2">
                Forgot password?
              </Link>
            </Grid>
            <Grid item>
              <Link to="/register" variant="body2">
                {"Don't have an account? Sign Up"}
              </Link>
            </Grid>
          </Grid>
        </Grid>
      </form>
    </Grid>
  )
}
