export const RESULT = {
  DRAW: 0,
  WIN: 1,
  LOSE: 2,
}
export const checkResult = (yourHand, opponentHand) => {
  if (yourHand - opponentHand === 0) return RESULT.DRAW
  if (yourHand - opponentHand === 1 || yourHand - opponentHand === -2)
    return RESULT.WIN
  return RESULT.LOSE
}
