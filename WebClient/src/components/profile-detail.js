import React from "react"
import {
  TextField,
  Grid,
  TableContainer,
  Paper,
  Table,
  TableHead,
  TableRow,
  TableBody,
  TableCell,
  TablePagination,
  CircularProgress,
} from "@material-ui/core"

export default function ProfileDetail({ data, isDisplayInfo }) {
  const [page, setPage] = React.useState(0)
  const [rowsPerPage, setRowsPerPage] = React.useState(10)
  if (data === null) {
    return <CircularProgress></CircularProgress>
  }
  const userInfo = data.user
  const gamesInfo = data.games
  const handleChangePage = p => {
    setPage(p)
  }
  const handleChangeRowsPerPage = e => {
    setRowsPerPage(parseInt(e.target.value, 10))
    setPage(0)
  }
  return (
    <div>
      {isDisplayInfo ? (
        <Grid container>
          <Grid item xs={4} md={2}>
            <TextField
              defaultValue="Username"
              disabled
              fullWidth
              variant="filled"
            />
          </Grid>
          <Grid item xs={8} md={10}>
            <TextField
              defaultValue={userInfo.username}
              disabled
              fullWidth
              variant="outlined"
            />
          </Grid>
          <Grid item xs={4} md={2}>
            <TextField
              defaultValue="Email"
              disabled
              fullWidth
              variant="filled"
            />
          </Grid>
          <Grid item xs={8} md={10}>
            <TextField
              defaultValue={userInfo.email}
              disabled
              fullWidth
              variant="outlined"
            />
          </Grid>
          <Grid item xs={4} md={2}>
            <TextField
              defaultValue="RegisteredAt"
              disabled
              fullWidth
              variant="filled"
            />
          </Grid>
          <Grid item xs={8} md={10}>
            <TextField
              defaultValue={userInfo.registeredAt}
              disabled
              fullWidth
              variant="outlined"
            />
          </Grid>
        </Grid>
      ) : (
        <Grid container>
          <TableContainer component={Paper}>
            <Table size="small" aria-label="a dense table">
              <TableHead>
                <TableRow>
                  <TableCell align="right">Game Id</TableCell>
                  <TableCell align="right">Result</TableCell>
                  <TableCell align="right">Date</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {gamesInfo
                  .slice(page * rowsPerPage, page * rowsPerPage + rowsPerPage)
                  .map((g, k) => (
                    <TableRow key={k}>
                      <TableCell align="right">{g.id}</TableCell>
                      <TableCell align="right">
                        {g.winnerId === -1 && <span>DRAW</span>}
                        {g.winnerId === userInfo.id && <span>WIN</span>}
                        {g.winnerId !== -1 && g.winnerId !== userInfo.id && (
                          <span>LOSE</span>
                        )}
                      </TableCell>
                      <TableCell align="right">
                        {new Date(g.gameDate).toLocaleString()}
                      </TableCell>
                    </TableRow>
                  ))}
              </TableBody>
            </Table>
          </TableContainer>
          <TablePagination
            rowsPerPageOptions={[10, 25]}
            component="div"
            count={gamesInfo.length}
            rowsPerPage={rowsPerPage}
            page={page}
            onChangePage={(e, p) => handleChangePage(p)}
            onChangeRowsPerPage={e => handleChangeRowsPerPage(e)}
          />
        </Grid>
      )}
    </div>
  )
}
