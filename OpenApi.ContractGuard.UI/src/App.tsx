import Logo from "./assets/logo.svg";
import "./App.css";
import { useState } from "react";
import { ContractComparisonResult } from "./components/ContractComperisonResult";

function App() {
  const applications = [
    "App1",
    "App2",
    "App3",
    "App4",
    "App5",
    "App6",
    "App7",
    "App8",
    "App9",
    "App10",
  ];

  const [selectedApp, setSelectedApp] = useState<string | null>(null);

  return (
    <div className="page">
      <div className="container">
        <header className="header fade-in">
          <img src={Logo} alt="OpenAPI Contract Tracker logo" className="logo" />
          <h1>OpenAPI Contract Tracker</h1>
          <p className="subtitle">Watching your API contracts</p>
        </header>

        <section className="content">
          <h2 className="section-title">Tracked applications</h2>

          <div className="app-list"

          >
            {applications.map((app, index) => (
              <div
                key={app}
                className="app-card slide-up"
                style={{ animationDelay: `${index * 60}ms` }}
                onClick={() => setSelectedApp(app)}
              >
                {app}

              </div>
            ))}
          </div>
        </section>
        {selectedApp && (
          <ContractComparisonResult
            appName={selectedApp}
            beforeVersion="v1.2.0"
            afterVersion="v1.3.0"
            breakingChanges={[
              { type: "breaking", message: "Removed field `userId` from /orders" },
              { type: "breaking", message: "POST /login now requires MFA" },
            ]}
            informationalChanges={[
              { type: "info", message: "Added optional field `nickname`" },
              { type: "info", message: "Updated response description for /users" },
            ]}
          />
        )}
      </div>
    </div>
  );
}

export default App;