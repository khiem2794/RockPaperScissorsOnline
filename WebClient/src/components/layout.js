import React from "react"
import PropTypes from "prop-types"
import {
  Grid,
  AppBar,
  Toolbar,
  Typography,
  makeStyles,
} from "@material-ui/core"
import { Link } from "gatsby"

const useStyles = makeStyles(theme => ({
  link: {
    textDecoration: "none",
    color: "black",
  },
}))

const Layout = ({ children }) => {
  const classes = useStyles()
  return (
    <Grid container justify="center">
      <Grid item xs={12} md={8} lg={7} xl={6}>
        <AppBar position="static">
          <Toolbar>
            <Link to="/" className={classes.link}>
              <Typography variant="h5">HOME</Typography>
            </Link>
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
