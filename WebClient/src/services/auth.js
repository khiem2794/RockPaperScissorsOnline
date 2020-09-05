import axios from "axios"

const authenticationHost = "https://localhost:6001/"
const profileHost = "https://localhost:7001/"

export default class UserAuth {
  constructor(username, accessToken, refreshToken, expireAt) {
    this.username = username
    this.accessToken = accessToken
    this.refreshToken = refreshToken
    this.expireAt = expireAt
    this.axiosConfig = {
      headers: {
        Authorization: "Bearer " + this.accessToken,
      },
    }
  }
  getProfile = () => {
    return axios
      .get(profileHost + "profile/info", this.axiosConfig)
      .then(({ data }) => {
        return data
      })
      .catch(err => {
        throw err
      })
  }
  refresh = () => {
    return axios
      .post(
        authenticationHost + "user/refresh",
        { refreshToken: this.refreshToken },
        this.axiosConfig
      )
      .then(({ data }) => {
        this.accessToken = data.accessToken
        this.refreshToken = data.refreshToken
        const jwtToken = JSON.parse(atob(data.accessToken.split(".")[1]))
        this.expireAt = new Date(jwtToken.exp * 1000).getTime()
      })
      .catch(err => {
        throw err
      })
  }
  logout = () => {
    axios.post(authenticationHost + "user/logout", null, this.axiosConfig)
  }
}

export const handleRegister = async (username, email, password) => {
  const postData = {
    username: username,
    email: email,
    password: password,
  }
  return await axios
    .post(authenticationHost + "user/register", postData)
    .then(({ data }) => {
      const jwtToken = JSON.parse(atob(data.accessToken.split(".")[1]))
      const user = new UserAuth(
        data.username,
        data.accessToken,
        data.refreshToken,
        new Date(jwtToken.exp * 1000).getTime()
      )
      return user
    })
    .catch(err => {
      throw err
    })
}

export const handleLogin = async (username, password) => {
  const postData = {
    username: username,
    password: password,
  }
  return await axios
    .post(authenticationHost + "user/login", postData)
    .then(({ data }) => {
      const jwtToken = JSON.parse(atob(data.accessToken.split(".")[1]))
      const user = new UserAuth(
        data.username,
        data.accessToken,
        data.refreshToken,
        new Date(jwtToken.exp * 1000).getTime()
      )
      return user
    })
    .catch(err => {
      throw err
    })
}

export const handleLoad = () => {
  return null
}
