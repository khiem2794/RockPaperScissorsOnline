import React, { useContext, useState, useEffect } from "react"
import { graphql, navigate } from "gatsby"
import Layout from "../components/layout"
import { AppContext, GAME_STATE, HAND } from "../context/context"
import { checkResult, RESULT, isBrowser } from "../helper/helper"
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
  const { state, createGame, joinWaitingGame, playHand, leftGame } = useContext(
    AppContext
  )
  const [currentHand, setCurrentHand] = useState(HAND.DEFAULT)
  const [renderState, setRenderState] = useState(state)
  const [endGameTimeoutId, setEndGameTimeoutId] = useState(-1)
  const [renderTimeoutId, setRenderTimeoutId] = useState(-1)
  const [isDelayRenderState, setIsDelayRenderState] = useState(false)

  useEffect(() => {
    if (state.game.gameState === GAME_STATE.COMPARE) {
      if (!isDelayRenderState) {
        setIsDelayRenderState(true)
        const id = setTimeout(() => {
          setRenderState(state)
          setIsDelayRenderState(false)
        }, 1500)
        setRenderTimeoutId(id)
      }
    }
    if (!isDelayRenderState) {
      setRenderState(state)
    }
  }, [state, isDelayRenderState])

  useEffect(() => {
    if (renderState.game.gameState === GAME_STATE.START) {
      setCurrentHand(HAND.DEFAULT)
    }
    if (renderState.game.gameState === GAME_STATE.END) {
      const id = setTimeout(() => endGame(), 3000)
      setEndGameTimeoutId(id)
    }
  }, [renderState.game.gameState])

  const create = () => {
    createGame()
  }

  const join = () => {
    joinWaitingGame()
  }

  const pickHand = hand => {
    setCurrentHand(hand)
    playHand(hand)
  }

  const endGame = () => {
    clearTimeout(endGameTimeoutId)
    clearTimeout(renderTimeoutId)
    setIsDelayRenderState(false)
    leftGame(renderState.game.gameId)
  }

  if (!renderState.auth.isAuthenticated) {
    if (isBrowser()) navigate("/login")
    return (
      <Layout>
        <div></div>
      </Layout>
    )
  }

  if (renderState.game.gameState === GAME_STATE.NONE) {
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
      {renderState.game.gameState === GAME_STATE.WAITING ? (
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
                name={renderState.game.opponent.name}
                score={renderState.game.opponent.point}
              />
            </Grid>
            <Grid container justify="center">
              {![GAME_STATE.COMPARE, GAME_STATE.END].includes(
                renderState.game.gameState
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
                    if (h.type === renderState.game.opponent.hand) {
                      return (
                        <Grid item xs={4} key={k}>
                          <HandCard
                            img={h.avatar}
                            type={h.type}
                            isDisabled={true}
                          />
                        </Grid>
                      )
                    }
                    return <div key={k}></div>
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
            {renderState.game.gameState === GAME_STATE.WAITING && (
              <span>WAITING FOR OPPONENT</span>
            )}

            {renderState.game.gameState === GAME_STATE.START && (
              <span>ROUND {renderState.game.round}</span>
            )}

            {renderState.game.gameState === GAME_STATE.COMPARE && (
              <span>
                {checkResult(
                  renderState.game.you.hand,
                  renderState.game.opponent.hand
                ) === RESULT.DRAW && (
                  <span>YOU DRAW ROUND {renderState.game.round}</span>
                )}
                {checkResult(
                  renderState.game.you.hand,
                  renderState.game.opponent.hand
                ) === RESULT.LOSE && (
                  <span>YOU LOSE ROUND {renderState.game.round}</span>
                )}
                {checkResult(
                  renderState.game.you.hand,
                  renderState.game.opponent.hand
                ) === RESULT.WIN && (
                  <span>YOU WIN ROUND {renderState.game.round}</span>
                )}
              </span>
            )}

            {renderState.game.gameState === GAME_STATE.END &&
              renderState.game.opponent.leftGame && (
                <span>YOU WIN (opponent left)</span>
              )}

            {renderState.game.gameState === GAME_STATE.END &&
              !renderState.game.opponent.leftGame && (
                <span>
                  {renderState.game.you.point >
                    renderState.game.opponent.point && <span>YOU WIN</span>}
                  {renderState.game.you.point <
                    renderState.game.opponent.point && <span>YOU LOSE</span>}
                  {renderState.game.you.point ===
                    renderState.game.opponent.point && <span>DRAW</span>}
                </span>
              )}
          </Typography>
        </Grid>
        <Grid item xs={1} md={3} className={classes.divide}>
          <Divider />
        </Grid>
      </Grid>

      <div>
        <Grid container justify="center">
          {![GAME_STATE.WAITING, GAME_STATE.END].includes(
            renderState.game.gameState
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
                    if (h.type === currentHand) {
                      return (
                        <Grid item xs={4} key={k}>
                          <HandCard
                            img={h.avatar}
                            type={h.type}
                            isDisabled={true}
                            isSelected={true}
                            isLost={
                              renderState.game.gameState === GAME_STATE.COMPARE
                                ? checkResult(
                                    renderState.game.you.hand,
                                    renderState.game.opponent.hand
                                  ) === RESULT.LOSE
                                : false
                            }
                          />
                        </Grid>
                      )
                    }
                    return <div key={k}></div>
                  })}
            </Grid>
          )}
          <Grid item xs={12}>
            <PlayerCard
              img={playerAvatar}
              name={renderState.game.you.name}
              score={renderState.game.you.point}
            />
          </Grid>
          <Button
            color="secondary"
            variant="contained"
            onClick={() => endGame()}
          >
            LEAVE
          </Button>
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
