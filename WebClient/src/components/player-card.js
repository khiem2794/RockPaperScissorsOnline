import React from "react"
import Img from "gatsby-image"
import { Card, Typography, Grid, makeStyles, Divider } from "@material-ui/core"

const useStyles = makeStyles(theme => ({
  root: {
    margin: "20px",
  },
  text: {
    display: "flex",
    flexDirection: "column",
    justifyContent: "center",
    textAlign: "center",
  },
}))

export default function PlayerCard({ img, score, name }) {
  const classes = useStyles()
  return (
    <Grid container justify="center">
      <Grid item xs={8} sm={5} lg={4}>
        <Card className={classes.root}>
          <Grid container>
            <Grid item xs={4} sm={5} lg={6}>
              <Img fluid={img.childImageSharp.fluid} />
            </Grid>
            <Divider orientation="vertical" flexItem />
            <Grid item xs={7} sm={6} lg={5} className={classes.text}>
              <Typography gutterBottom variant="h5" component="h5">
                {name}
              </Typography>
              <Divider variant="fullWidth" />
              <Typography gutterBottom variant="h4" component="h5">
                {score}
              </Typography>
            </Grid>
          </Grid>
        </Card>
      </Grid>
    </Grid>
  )
}
