import React, { useContext, useState, useEffect } from "react"
import { graphql } from "gatsby"
import Layout from "../components/layout"
import { AppContext, GAME_STATE, HAND } from "../context/context"
import { checkResult, RESULT } from "../helper/helper"
import {
  Grid,
  Divider,
  Typography,
  makeStyles,
  Button,
} from "@material-ui/core"
import PlayerCard from "../components/player-card"
import HandCard from "../components/hand-card"

const useStyles = makeStyles(theme => ({
  divide: {
    display: "flex",
    flexDirection: "column",
    justifyContent: "center",
    textAlign: "center",
  },
  createJoin: {
    marginTop: "200px",
  },
  createJoinButton: {
    marginTop: "20px",
  },
}))

export default function Index({ data }) {
  const classes = useStyles()
  const imgs = data.allFile.edges
  const playerAvatar = imgs.find(e => e.node.name === "player-avatar").node
  const opponentAvatar = imgs.find(e => e.node.name === "opponent-avatar").node
  const rockAvatar = imgs.find(e => e.node.name === "rock").node
  const paperAvatar = imgs.find(e => e.node.name === "paper").node
  const scissorsAvatar = imgs.find(e => e.node.name === "scissors").node
  const handCollection = [
    { type: HAND.ROCK, avatar: rockAvatar },
    { type: HAND.PAPER, avatar: paperAvatar },
    { type: HAND.SCISSORS, avatar: scissorsAvatar },
  ]
  const [currentHand, setCurrentHand] = useState(HAND.DEFAULT)
  const { state, createGame, joinGame, playHand, leftGame } = useContext(
    AppContext
  )

  useEffect(() => {
    if (state.game.gameState === GAME_STATE.START) {
      setCurrentHand(HAND.DEFAULT)
    }
    if (state.game.gameState === GAME_STATE.END) {
      setTimeout(() => endGame(), 3000)
    }
  }, [state.game.gameState])

  const create = () => {
    createGame()
  }

  const join = () => {
    joinGame("123")
  }

  const pickHand = hand => {
    setCurrentHand(hand)
    playHand(hand)
  }

  const endGame = () => {
    leftGame(state.game.gameId)
  }

  if (state.game.gameState === GAME_STATE.NONE) {
    return (
      <Layout>
        <Grid container justify="center" className={classes.createJoin}>
          <Grid item xs={12} sm={6}>
            <Grid container justify="center">
              <Button
                variant="contained"
                color="primary"
                onClick={e => create()}
                className={classes.createJoinButton}
              >
                <Typography variant="h5" component="h4">
                  CREATE GAME
                </Typography>
              </Button>
            </Grid>
          </Grid>
          <Grid item xs={12} sm={6}>
            <Grid container justify="center">
              <Button
                variant="contained"
                color="primary"
                className={classes.createJoinButton}
                onClick={e => join()}
              >
                <Typography variant="h5" component="h4">
                  JOIN GAME
                </Typography>
              </Button>
            </Grid>
          </Grid>
        </Grid>
      </Layout>
    )
  }

  return (
    <Layout>
      {state.game.gameState === GAME_STATE.WAITING ? (
        <div>
          <Grid container justify="center">
            <PlayerCard img={opponentAvatar} name="?" score="0" />
          </Grid>
        </div>
      ) : (
        <div>
          <Grid container justify="center">
            <Grid item xs={12}>
              <PlayerCard
                img={opponentAvatar}
                name={state.game.opponent.name}
                score={state.game.opponent.point}
              />
            </Grid>
            <Grid container justify="center">
              {![GAME_STATE.COMPARE, GAME_STATE.END].includes(
                state.game.gameState
              )
                ? handCollection.map((h, k) => (
                    <Grid item xs={4} key={k}>
                      <HandCard
                        img={h.avatar}
                        type={h.type}
                        isDisabled={true}
                      />
                    </Grid>
                  ))
                : handCollection.map((h, k) => {
                    if (h.type === state.game.opponent.hand)
                      return (
                        <Grid item xs={4} key={k}>
                          <HandCard
                            img={h.avatar}
                            type={h.type}
                            isDisabled={true}
                          />
                        </Grid>
                      )
                  })}
            </Grid>
          </Grid>
        </div>
      )}

      <Grid container style={{ margin: "20px" }}>
        <Grid item xs={2} md={4} className={classes.divide}>
          <Divider />
        </Grid>
        <Grid item xs={8} md={4} className={classes.divide}>
          <Typography gutterBottom variant="h5" component="h5">
            {state.game.gameState === GAME_STATE.WAITING && (
              <span>WAITING FOR OPPONENT</span>
            )}

            {state.game.gameState === GAME_STATE.START && (
              <span>ROUND {state.game.round}</span>
            )}

            {state.game.gameState === GAME_STATE.COMPARE &&
              checkResult(state.game.you.hand, state.game.opponent.hand) ===
                RESULT.DRAW && <span>YOU DRAW ROUND {state.game.round}</span>}

            {state.game.gameState === GAME_STATE.COMPARE &&
              checkResult(state.game.you.hand, state.game.opponent.hand) ===
                RESULT.WIN && <span>YOU WIN ROUND {state.game.round}</span>}

            {state.game.gameState === GAME_STATE.COMPARE &&
              checkResult(state.game.you.hand, state.game.opponent.hand) ===
                RESULT.LOSE && <span>YOU LOSE ROUND {state.game.round}</span>}

            {state.game.gameState === GAME_STATE.END && (
              <span>
                {state.game.you.point > state.game.opponent.point && (
                  <span>YOU WIN</span>
                )}
                {state.game.you.point < state.game.opponent.point && (
                  <span>YOU LOSE</span>
                )}
                {state.game.you.point === state.game.opponent.point && (
                  <span>DRAW</span>
                )}
              </span>
            )}
          </Typography>
        </Grid>
        <Grid item xs={2} md={4} className={classes.divide}>
          <Divider />
        </Grid>
      </Grid>

      <div>
        <Grid container justify="center">
          {![GAME_STATE.WAITING, GAME_STATE.END].includes(
            state.game.gameState
          ) && (
            <Grid container justify="center">
              {currentHand === HAND.DEFAULT
                ? handCollection.map((h, k) => (
                    <Grid item xs={4} onClick={() => pickHand(h.type)} key={k}>
                      <HandCard
                        img={h.avatar}
                        type={h.type}
                        isDisabled={false}
                      />
                    </Grid>
                  ))
                : handCollection.map((h, k) => {
                    if (h.type === currentHand)
                      return (
                        <Grid item xs={4} key={k}>
                          <HandCard
                            img={h.avatar}
                            type={h.type}
                            isDisabled={true}
                            isSelected={true}
                            isLost={
                              state.game.gameState === GAME_STATE.COMPARE
                                ? checkResult(
                                    state.game.you.hand,
                                    state.game.opponent.hand
                                  ) === RESULT.LOSE
                                : false
                            }
                          />
                        </Grid>
                      )
                  })}
            </Grid>
          )}
          <Grid item xs={12}>
            <PlayerCard
              img={playerAvatar}
              name={state.game.you.name}
              score={state.game.you.point}
            />
          </Grid>
        </Grid>
      </div>
    </Layout>
  )
}

export const query = graphql`
  {
    allFile {
      edges {
        node {
          name
          childImageSharp {
            fluid {
              ...GatsbyImageSharpFluid
            }
          }
        }
      }
    }
  }
`
