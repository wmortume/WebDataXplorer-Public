import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "primereact/resources/themes/viva-dark/theme.css"; // Prime Theme CSS
import "primereact/resources/primereact.min.css"; // Prime Core CSS
import "primeicons/primeicons.css"; // Prime Icons
import "primeflex/primeflex.css"; // Prime CSS Utility
import App from "./App.jsx";

createRoot(document.getElementById("root")).render(
    <StrictMode>
        <App />
    </StrictMode>
);
