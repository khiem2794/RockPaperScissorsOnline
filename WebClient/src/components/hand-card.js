import React from "react"
import Img from "gatsby-image"
import { Card, CardActionArea, Grid, makeStyles } from "@material-ui/core"

const useStyles = makeStyles(theme => ({
  root: {
    padding: "5px",
  },
  hand: {
    display: "flex",
    flexDirection: "column",
    justifyContent: "center",
    [theme.breakpoints.down("sm")]: {
      height: 150,
    },
    [theme.breakpoints.up("sm")]: {
      height: 150,
    },
    [theme.breakpoints.up("md")]: {
      height: 160,
    },
    [theme.breakpoints.up("lg")]: {
      height: 170,
    },
    [theme.breakpoints.up("xl")]: {
      height: 180,
    },
  },
}))

export default function HandCard({
  img,
  type,
  isDisabled,
  isSelected,
  isLost,
}) {
  const classes = useStyles()
  return (
    <Grid container justify="center" className={classes.root}>
      <Grid item xs={12} sm={8} md={8} lg={7} xl={6}>
        <Card>
          <CardActionArea
            disabled={isDisabled}
            style={{
              backgroundColor: isLost ? "red" : isSelected ? "green" : "white",
            }}
          >
            <Grid container justify="center">
              <Grid item xs={10} lg={8} className={classes.hand}>
                <Img fluid={img.childImageSharp.fluid} />
              </Grid>
            </Grid>
          </CardActionArea>
        </Card>
      </Grid>
    </Grid>
  )
}
