const gateway = "https://localhost:3001/"

const ApiResources = {
  getJwt: gateway + "user/jwt",
  register: gateway + "user/register",
  login: gateway + "user/login",
  logout: gateway + "user/logout",
  refresh: gateway + "user/refresh",
  profile: gateway + "user/profile",
  gamehub: gateway + "gamehub",
}

export default ApiResources
