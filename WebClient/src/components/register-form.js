import React, { useState } from "react"
import { Grid, Button, TextField, Typography } from "@material-ui/core"

export default function RegisterForm({ handleRegister }) {
  const [userName, setUserName] = useState("")
  const [email, setEmail] = useState("")
  const [password, setPassword] = useState("")
  const [isRegistering, setIsRegistering] = useState(false)
  const submitRegister = e => {
    e.preventDefault()
    setIsRegistering(true)
    try {
      handleRegister(userName, email, password)
    } catch (error) {
      setIsRegistering(false)
    }
  }
  return (
    <Grid item xs={12}>
      <form autoComplete="off" onSubmit={e => submitRegister(e)}>
        <Grid container justify="center">
          <Typography component="h1" variant="h5">
            Register new account
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
              type="text"
              name="email"
              label="Email"
              fullWidth
              margin="normal"
              InputLabelProps={{
                shrink: true,
              }}
              variant="outlined"
              onChange={e => setEmail(e.target.value)}
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
              disabled={isRegistering}
            >
              REGISTER
            </Button>
          </Grid>
        </Grid>
      </form>
    </Grid>
  )
}
