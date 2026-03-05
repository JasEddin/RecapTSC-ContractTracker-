import Logo from "./assets/logo.svg";
import "./App.css";
import { useEffect, useState } from "react";
import { ContractComparisonResult } from "./components/ContractComperisonResult";

type Application = {
  changeImpact: 0 | 1;
  name: string;
  team: { name: string, mail: string };
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
  const [selectedTeam, setSelectedTeam] = useState<string>("ALL");
  const [showHeader, setShowHeader] = useState(true);

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
      fetch(`http://localhost:5093/api/application/${selectedApp.name}`)
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

const filteredApplications =
  selectedTeam === "ALL"
    ? applications
    : applications.filter(app => app.team.name.toLowerCase() === selectedTeam.toLowerCase());

const uniqueTeams = Array.from(
  new Map(
    applications.map(app => [
      app.team.name.toLowerCase(),
      {
        name: app.team.name,
        mail: app.team.mail
      }
    ])
  ).values()
).sort((a, b) => a.name.localeCompare(b.name));

const capitalizeWords = (text: string) =>
  text
    .split(" ")
    .map(word => word.charAt(0).toUpperCase() + word.slice(1))
    .join(" ");
  debugger;
  return (
    <>
      {!selectedApp && showHeader && (
        <div className="page">
          <div className="container">
            <header className="header fade-in">
              <img src={Logo} className="logo" />
              <h1>OpenAPI Contract Tracker</h1>
              <p className="subtitle">Watching your API contracts</p>
            </header>
            <div className="content fade-in">
              <h2 className="section-title">{`Tracked applications (${filteredApplications.length})`}</h2>
              <select
                className="team-select"
                value={selectedTeam}
                onChange={(e) => {
                  setSelectedTeam(e.target.value);
                  setSelectedApp(null);
                  setApplicationDetails(null);
                  setShowHeader(true);
                }}
              >
                <option value="ALL">All Teams</option>

                {uniqueTeams.map(team => (
                      <option key={team.name} value={team.name}>
                        {capitalizeWords(team.name)}
                  </option>
                ))}
              </select>
            </div>
            {loading && <p>Loading applications...</p>}
            {error && <p>Error: {error}</p>}
            {!loading && !error && (
              <>

                <div className="app-list">
                  {filteredApplications.map((app, index) => (
                    <div
                      key={app.name}
                      className={`app-card slide-up ${app.changeImpact === 1 ? "critical" : ""}`}
                      style={{ animationDelay: `${index * 60}ms` }}
                      onClick={() => setSelectedApp(app)}
                    >
                      {app.name}
                    </div>
                  ))}
                </div>
              </>
            )}
          </div>
        </div>
      )}

      {(selectedApp || !showHeader) && !loading && !error && (
        <div className="page">
          <div className="layout">
            {/* ASIDE / LEFT MENU */}
            <aside className="sidebar">
              <div className="sidebar-header">
                <img src={Logo} className="sidebar-logo" />
                <span className="sidebar-title"> {`Applications (${filteredApplications.length})`} </span>
             </div>
                <select
                 className="team-select sidebar-select"
                  value={selectedTeam}
                  onChange={(e) => {
                    setSelectedTeam(e.target.value);
                    setSelectedApp(null);
                    setApplicationDetails(null);
                    setShowHeader(false);
                  }}
                >
                  <option value="ALL">All Teams</option>
                  {uniqueTeams.map(team => (
                    <option key={team.name} value={team.name}>
                      {capitalizeWords(team.name)}
                    </option>
                  ))}
                </select>
          

              <div className="sidebar-list">
                {filteredApplications.map((app) => (
                  <div
                    key={app.name}
                    className={`sidebar-item ${selectedApp === app ? "active" : ""
                      }`}
                    onClick={() => setSelectedApp(app)}
                  >
                    <span className="sidebar-icon">
                      {app.changeImpact === 1 ? "🛑" : "🟢"}
                    </span>
                    <span className="sidebar-text">
                      {app.name}
                    </span>
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