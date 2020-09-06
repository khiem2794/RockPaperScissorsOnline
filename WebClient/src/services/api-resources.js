const gateway = "https://localhost:9001/"

const ApiResources = {
  register: gateway + "user/register",
  login: gateway + "user/login",
  logout: gateway + "user/logout",
  refresh: gateway + "user/refresh",
  profile: gateway + "user/profile",
  gamehub: gateway + "gamehub",
}

export default ApiResources
