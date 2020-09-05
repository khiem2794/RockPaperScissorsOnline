import React, { useEffect } from "react"
import Layout from "../components/layout"
import { Grid, makeStyles } from "@material-ui/core"
import ProfileSideBar from "../components/profile-sidebar"
import ProfileDetail from "../components/profile-detail"
import { useContext } from "react"
import { AppContext } from "../context/context"
import { navigate } from "gatsby"
import { useState } from "react"
import { isBrowser } from "../helper/helper"

const useStyles = makeStyles(theme => ({
  root: {
    marginTop: "50px",
  },
}))

export default function Profile() {
  const classes = useStyles()
  const { state, getProfile } = useContext(AppContext)
  const [profileDetail, setProfileDetail] = useState(null)
  const [displayInfo, setDisplayInfo] = useState(true)
  useEffect(() => {
    const getProfileInfo = async () => {
      if (state.auth.isAuthenticated) {
        getProfile()
          .then(data => {
            setProfileDetail(data)
          })
          .catch(err => console.error(err))
      }
    }
    getProfileInfo()
  }, [])
  if (!state.auth.isAuthenticated) {
    if (isBrowser()) navigate("/login")
    return <Layout>Required Login</Layout>
  }
  const showUserInfo = () => {
    if (displayInfo !== true) setDisplayInfo(value => !value)
  }
  const showUserGames = () => {
    if (displayInfo === true) setDisplayInfo(value => !value)
  }
  return (
    <Layout>
      <Grid container className={classes.root} justify="center">
        <Grid item xs={3} md={3}>
          <ProfileSideBar
            showUserInfo={() => showUserInfo()}
            showUserGames={() => showUserGames()}
          />
        </Grid>
        <Grid item xs={1}></Grid>
        <Grid item xs={8} md={8}>
          <ProfileDetail data={profileDetail} isDisplayInfo={displayInfo} />
        </Grid>
      </Grid>
    </Layout>
  )
}
