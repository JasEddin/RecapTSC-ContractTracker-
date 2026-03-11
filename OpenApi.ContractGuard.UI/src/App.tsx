import Logo from "./assets/logo.svg";
import "./App.css";
import { useEffect, useState } from "react";
import { ContractComparisonResult } from "./components/ContractComperisonResult";

type Application = {
  changeImpact: 0 | 1;
  name: string;
  team: { name: string, mail: string };
}
type Environment = "u3" | "u4" | "u5";

export type ApplicationDetails = {
  name: string;
  server: string;
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

  const [applications , setApplications ] = useState<Record<Environment, Application[]>>({
    u3: [],
    u4: [],
    u5: []
  });



  const [applicationDetails, setApplicationDetails] = useState<Record<Environment, ApplicationDetails | null>>({
    u3: null ,
    u4: null,
    u5: null
  });

  const [selectedEnv, setSelectedEnv] = useState<Environment>("u3");
  const [selectedApp, setSelectedApp] = useState<Application | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedTeam, setSelectedTeam] = useState<string>("ALL");
  const [showHeader, setShowHeader] = useState(true);
  const [isFilteredByCritical, setIsFilteredByCritical] = useState<boolean>(false);

  useEffect(() => {
    setLoading(true);
    fetch(`http://localhost:5093/api/applications/${selectedEnv}`)
      .then((res) => {
        if (!res.ok) throw new Error("Failed to load applications");
        return res.json();
      })
      .then((data) => {

         
        setApplications(prev => ({ ...prev, [selectedEnv]: data }));
      })
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }, [selectedEnv]);


  useEffect(() => {
    if (selectedApp) {
      document.title = `${selectedApp.name} - OpenAPI Contract Tracker`;
      fetch(`http://localhost:5093/api/application/q?name=${selectedApp.name}&env=${selectedEnv}`)
        .then((res) => {
          if (!res.ok) throw new Error("Failed to load application details");
          return res.json();
        })
        .then((data) => {
          setApplicationDetails(prev => ({ ...prev, [selectedEnv]: data }));
        })
        .catch((err) => setError(err.message));
    }
  }, [selectedApp, selectedEnv]);

  const filteredApplicationsByTeam =
    selectedTeam === "ALL"
      ? applications[selectedEnv]
      : applications[selectedEnv].filter(app => app.team.name.toLowerCase() === selectedTeam.toLowerCase());

  const higherFilteredApplications = isFilteredByCritical
    ? filteredApplicationsByTeam.filter(app => app.changeImpact === 1)
    : filteredApplicationsByTeam;

  const filteredByCriticalOnly = isFilteredByCritical
    ? applications[selectedEnv].filter(app => app.changeImpact === 1)
    : applications[selectedEnv];

  const uniqueTeams = Array.from(
    new Map(
      filteredByCriticalOnly.map(app => [
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
              <h2 className="section-title">{`Tracked applications (${higherFilteredApplications.length})`}
                <button className="critical-filter-btn" style={{ background: isFilteredByCritical ? "grey" : "#f9f9f9", marginLeft: "8px" }} onClick={() => setIsFilteredByCritical(!isFilteredByCritical)} >  🔴 </button>
              </h2>
              <span className="filter-icon">
              </span>
              <select
                className="team-select"
                value={selectedTeam}
                onChange={(e) => {
                  setSelectedTeam(e.target.value);
                  setSelectedApp(null);
                  setApplicationDetails(prev => ({ ...prev, [selectedEnv]: null }));
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
            {loading && <h2> ⏳ Loading applications...</h2>}
            {error && <p>Error: {error}</p>}
            {!loading && !error && (
              <>
                <div className="app-list">
                  {higherFilteredApplications.map((app, index) => (
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
              <div className="sidebar-background" >
                <div className="sidebar-header" >
                  <img style={{ position: "fixed", top: "20px", left: "20px" }} src={Logo} className="sidebar-logo" />
                  <span style={{ position: "fixed", top: "20px", left: "60px" }} className="sidebar-title"> {`Applications (${higherFilteredApplications.length})`} </span>
                  <span className="filter-icon sidebar-filter-icon" style={{ position: "fixed", top: "20px", left: "440px" }}>
                    <button className="critical-filter-btn" style={{ background: isFilteredByCritical ? "grey" : "#020617", marginLeft: "8px" }} onClick={() => setIsFilteredByCritical(!isFilteredByCritical)} >  🔴 </button>
                  </span>
                </div>
                <select  style={{ position: "fixed", top: "68px" ,width: "468px"}}
                  className="team-select sidebar-select"
                  value={selectedTeam}
                  onChange={(e) => {
                    setSelectedTeam(e.target.value);
                    setSelectedApp(null);
                    setApplicationDetails(prev => ({ ...prev, [selectedEnv]: null }));
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
              </div>
              <div className="sidebar-list">
                {higherFilteredApplications.map((app) => (
                  <div
                    key={app.name}
                    className={`sidebar-item ${selectedApp === app ? "active" : ""
                      }`}
                    onClick={() => setSelectedApp(app)}
                  >
                    <span className="sidebar-icon">
                      {app.changeImpact === 1 ? "🔴" : "🟢"}
                    </span>
                    <span className="sidebar-text">
                      {app.name}
                    </span>
                  </div>
                ))}
              </div>
            </aside>
            {selectedApp && applicationDetails[selectedEnv]  ? (

              <main className="main"  >
                <ContractComparisonResult {...applicationDetails[selectedEnv]!} />
              </main>

            ) : (
              <div className="main placeholder" style={{paddingTop:"150px"}}>
                <h2>⬅️ Select an application to view details</h2>
              </div>
            )}
          </div>
        </div>

      )}
    </>
  );
}


export default App;