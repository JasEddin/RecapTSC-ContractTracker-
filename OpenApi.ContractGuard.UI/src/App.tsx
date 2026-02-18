import Logo from "./assets/logo.svg";
import "./App.css";
import { useEffect, useState } from "react";
import { ContractComparisonResult } from "./components/ContractComperisonResult";

type Application = {
  id: string;
  name: string;
}

export type ApplicationDetails = {
  name: string;
  url: string;
  localContractPath: string;
  changes: {
    changeType: number;
    path: string;
    operation: string;
    message: string;
    impact: number;
  }[];
};

function App() {

  const [applications, setApplications] = useState<Application[]>([]);
  const [applicationDetails, setApplicationDetails] = useState<ApplicationDetails | null>(null);
  const [selectedApp, setSelectedApp] = useState<Application | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetch("http://localhost:5093/api/applications")
      .then((res) => {
        if (!res.ok) throw new Error("Failed to load applications");
        return res.json();
      })
      .then(setApplications)
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }, []);

  useEffect(() => {
    if (selectedApp) {
      document.title = `${selectedApp.name} - OpenAPI Contract Tracker`;
      fetch(`http://localhost:5093/api/application/${selectedApp.id}`)
        .then((res) => {
          if (!res.ok) throw new Error("Failed to load application details");
          return res.json();
        })
        .then((data) => {
          setApplicationDetails(data);
        })
        .catch((err) => setError(err.message));
    }
  }, [selectedApp]);


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

            {loading && <p>Loading applications...</p>}
            {error && <p>Error: {error}</p>}
            {!loading && !error && (
              <div className="app-list">
                {applications.map((app, index) => (
                  <div
                    key={app.id}
                    className="app-card slide-up"
                    style={{ animationDelay: `${index * 60}ms` }}
                    onClick={() => setSelectedApp(app)}
                  >
                    {app.name}
                  </div>
                ))}
              </div>)}
          </div>
        </div>
      )}

      {selectedApp && !loading && !error && (
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
                    key={app.id}
                    className={`sidebar-item ${selectedApp === app ? "active" : ""
                      }`}
                    onClick={() => setSelectedApp(app)}
                  >
                    {app.name}
                  </div>
                ))}
              </div>
            </aside>
            {selectedApp && applicationDetails ? (

              <main className="main">
                <ContractComparisonResult {...applicationDetails!} />
              </main>

            ) : (
              <div className="main-placeholder">
                <p>Select an application to view details</p>
              </div>
            )}
          </div>
        </div>

      )}
    </>
  );
}


export default App;