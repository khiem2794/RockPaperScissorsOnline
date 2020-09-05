import React from "react"
import {
  ListItem,
  List,
  makeStyles,
  Divider,
  Typography,
} from "@material-ui/core"

const useStyles = makeStyles(theme => ({
  root: {
    width: "100%",
    backgroundColor: theme.palette.background.default,
  },
}))

export default function ProfileSideBar({ showUserGames, showUserInfo }) {
  const classes = useStyles()
  return (
    <div className={classes.root}>
      <List component="nav" aria-label="main mailbox folders">
        <ListItem button onClick={() => showUserInfo()}>
          <Typography variant="h5">Profile</Typography>
        </ListItem>
        <Divider />
        <ListItem button onClick={() => showUserGames()}>
          <Typography variant="h5">Games</Typography>
        </ListItem>
      </List>
    </div>
  )
}
