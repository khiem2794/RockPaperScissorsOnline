import React, { useContext } from "react"
import PropTypes from "prop-types"
import {
  Grid,
  AppBar,
  Toolbar,
  Typography,
  makeStyles,
  IconButton,
} from "@material-ui/core"
import ExitToAppIcon from "@material-ui/icons/ExitToApp"
import { Link } from "gatsby"
import { AppContext } from "../context/context"

const useStyles = makeStyles(theme => ({
  root: {
    // flexGrow: 1,
  },
  menuButton: {
    marginRight: theme.spacing(2),
  },
  title: {
    flexGrow: 1,
    textDecoration: "none",
    color: "black",
  },
  info: {
    display: "flex",
  },
}))

const Layout = ({ children }) => {
  const classes = useStyles()
  const { state, logout } = useContext(AppContext)
  console.log("stat", state)
  return (
    <Grid container justify="center">
      <Grid item xs={12} md={8} lg={7} xl={6} className={classes.root}>
        <AppBar position="static">
          <Toolbar>
            <Link to="/" className={classes.title}>
              <Typography variant="h5">HOME</Typography>
            </Link>
            {state.auth.isAuthenticated && (
              <Link to="/profile" className={classes.title}>
                <Typography variant="h5">PROFILE</Typography>
              </Link>
            )}
            {!state.auth.isAuthenticated && (
              <Link to="/login" className={classes.title}>
                <Typography variant="h5">LOGIN</Typography>
              </Link>
            )}
            {!state.auth.isAuthenticated && (
              <Link to="/register" className={classes.title}>
                <Typography variant="h5">REGISTER</Typography>
              </Link>
            )}
            {state.auth.isAuthenticated && (
              <div className={classes.info}>
                <Typography variant="h5">
                  {state.auth.userAuth.username}
                </Typography>
                <IconButton
                  component="span"
                  color="secondary"
                  size="small"
                  onClick={() => logout()}
                >
                  <ExitToAppIcon fontSize="large" />
                </IconButton>
              </div>
            )}
          </Toolbar>
        </AppBar>
        {children}
      </Grid>
    </Grid>
  )
}

Layout.propTypes = {
  children: PropTypes.node.isRequired,
}

export default Layout
