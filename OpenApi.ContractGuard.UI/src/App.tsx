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
    <>
      {!selectedApp && (
        <div className="page">
          <div className="container">
            <header className="header fade-in">
              <img src={Logo} className="logo" />
              <h1>OpenAPI Contract Tracker</h1>
              <p className="subtitle">Watching your API contracts</p>
            </header>

            <h2 className="section-title">Tracked applications</h2>

            <div className="app-list">
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
          </div>
        </div>
      )}

      {selectedApp && (
        <div className="page">
          <div className="layout">
            {/* ASIDE / LEFT MENU */}
            <aside className="sidebar">
              <div className="sidebar-header">
                <img src={Logo} className="sidebar-logo" />
                <span className="sidebar-title">Applications</span>
              </div>

              <div className="sidebar-list">
                {applications.map((app) => (
                  <div
                    key={app}
                    className={`sidebar-item ${selectedApp === app ? "active" : ""
                      }`}
                    onClick={() => setSelectedApp(app)}
                  >
                    {app}
                  </div>
                ))}
              </div>
            </aside>

            {/* MAIN CONTENT */}
            <main className="main">
              <ContractComparisonResult
                appName={selectedApp}
                beforeVersion="v1.2.0"
                afterVersion="v1.3.0"
                breakingChanges={[
                  {
                    type: "breaking",
                    message: "Removed field `userId` from /orders",
                  },
                ]}
                informationalChanges={[
                  {
                    type: "info",
                    message: "Added optional field `nickname`",
                  },
                ]}
              />
            </main>
          </div>
        </div>

      )}
    </>)
}


export default App;