const PROXY_CONFIG = [
  {
    context: [
      "/weatherforecast",
      '/identity'
    ],
    target: "https://localhost:7184",
    secure: false
  }
]

module.exports = PROXY_CONFIG;
