const apiPort = '5115'; // Cambia este puerto según la configuración de Firmeza.API (ej. 5115 para HTTP o 7170 para HTTPS)
const apiBaseUrl = `http://localhost:${apiPort}/api`;

export const environment = {
  production: false,
  apiPort,
  apiUrl: apiBaseUrl
};
