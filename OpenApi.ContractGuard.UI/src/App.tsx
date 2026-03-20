import Logo from "./assets/logo.svg";
import "./App.css";
import { useEffect, useState } from "react";
import { ContractComparisonResult } from "./components/ContractComperisonResult";

type Application = {
  changeImpact: 0 | 1 | 2;
  name: string;
  team: { name: string, mail: string };
}
export type Environment = "u3" | "u4" | "u5";

export const Environments: Environment[] = ["u3", "u4", "u5"];

export type ApplicationDetails = {
  name: string;
  server: string;
  localContractPath: string;
  changes: {
    changeType: 0 | 1 | 2;
    path: string;
    operation: string;
    message: string;
    impact: 0 | 1 | 2;
  }[];
};

function App() {

  const [applications, setApplications] = useState<Record<Environment | "noEnv", Application[]>>({
    noEnv: [],
    u3: [],
    u4: [],
    u5: []
  });



  const [applicationDetails, setApplicationDetails] = useState<Record<Environment, ApplicationDetails | null>>({
    u3: null,
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
    const loadApplications = async () => {
      try {
        const res = await fetch(`http://localhost:5093/api/applications/`);
        if (!res.ok) {
          throw new Error(`Failed loading applications`);
        }
        const data = await res.json();
        setApplications(prev => ({
          ...prev,
          ['noEnv']: data
        }));

        setLoading(false);

      }
      catch (err: any) {
        setError(err.message);
        setLoading(false);
      }
    };
    const loadEnvironments = async () => {
      try {
        setLoading(true);
        for (let i = 0; i < Environments.length; i++) {
          const env = Environments[i];
          const res = await fetch(`http://localhost:5093/api/applications/${env}`);
          if (!res.ok) {
            throw new Error(`Failed loading ${env}`);
          }
          const data = await res.json();
          setApplications(prev => ({
            ...prev,
            [env]: data
          }));
          // stop loading after first env
          if (i === 0) {
            setLoading(false);
          }
        }
      } catch (err: any) {
        setError(err.message);
        setLoading(false);
      }
    };
    loadApplications();
    loadEnvironments();
  }
    , []);


  useEffect(() => {

  }, []);


  const getAppDetails = (appName: string, env: Environment) => {
    // setSelectedEnv(env);
    document.title = `${appName} - OpenAPI Contract Tracker`;
    fetch(`http://localhost:5093/api/application?name=${appName}&environment=${env}`)
      .then(res => {
        if (!res.ok) throw new Error("Failed to load application details");
        return res.json();
      })
      .then(data => {
        setApplicationDetails(prev => ({ ...prev, [env]: data }));
      })
      .catch(err => setError(err.message));

  };

  const changeEnvironment = (env: Environment) => {
    setSelectedEnv(env);
    if (selectedApp) {
      getAppDetails(selectedApp.name, env);
    }
  };

  useEffect(() => {
    if (selectedApp) {
      getAppDetails(selectedApp.name, selectedEnv);
    }
  }, [selectedApp, selectedEnv]);

  const allAppsWithoutImpact = [...applications.noEnv];
  const allAppsWithImpact = [...applications.u3, ...applications.u4, ...applications.u5];

  const uniqueAppsMap = new Map<string, Application>();
  allAppsWithImpact.forEach(app => {
    if (!uniqueAppsMap.has(app.name)) {
      uniqueAppsMap.set(app.name, app);
    }
  });
  // order by name
  const uniqueApps = Array.from(uniqueAppsMap.values()).sort((a, b) => a.name.localeCompare(b.name));
  const filteredApplicationsByTeam =
    selectedTeam === "ALL"
      ? uniqueApps
      : uniqueApps.filter(app => app.team.name.toLowerCase() === selectedTeam.toLowerCase());

  const higherFilteredApplications = isFilteredByCritical
    ? filteredApplicationsByTeam.filter(app => app.changeImpact === 1)
    : filteredApplicationsByTeam;

  const uniqueTeams = Array.from(
    new Map(
      allAppsWithImpact.map(app => [
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

  const getBall = (env: Environment, appName: string) => {
    const app = applications[env].find(a => a.name === appName);

    if (!app) return (
      <span style={{ display: "inline-block", width: "12px", height: "12px", borderRadius: "6px", backgroundColor: "#d6d6d6" }}></span>
    );
    if (app.changeImpact === 1) {
      return (
        <span style={{ display: "inline-block", width: "12px", height: "12px", borderRadius: "6px", backgroundColor: "#d13333" }}></span>);
    } else
      if (app.changeImpact === 0) {
        return (<span style={{ display: "inline-block", width: "12px", height: "12px", borderRadius: "6px", backgroundColor: "#6d9f6d" }}></span>);
      }
      else {
        return (<span style={{ display: "inline-block", width: "12px", height: "12px", borderRadius: "6px", backgroundColor: "#868181" }}></span>);
      }
  };
  const getColorOfTab: (env: Environment, appName: string) => string = (env: Environment, appName: string) => {
    const app = applications[env].find(a => a.name === appName);
    if (app?.changeImpact == 2) return "grey";
    return app?.changeImpact === 1 ? "#dc4d4d" : "#8ad58a";
  }

  const selectAppInEnv = (env: Environment, name: string) => {
    const app = applications[env].find(app => app.name === name);
    setSelectedEnv(env);
    setSelectedApp(app || null);
  };
  const isApplicationExistInAnyEnv = (appName: string) => {

    return allAppsWithImpact.some(app => app.name === appName && app.changeImpact !== 2);
  }
  const selectAppInAnyEnv = (appName: string) => {
    if (isApplicationExistInAnyEnv(appName)) {
      const env = Environments.find(env => applications[env].find(a => a.name === appName && (a.changeImpact === 1 || a.changeImpact === 0)))!;
      selectAppInEnv(env, appName);
    }
  }
  const isDisabled = (env: Environment, appName: string) => {
    return !applications[env].find(a => a.name === appName) || applications[env].find(a => a.name === appName)?.changeImpact === 2;
  }


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
              <h2 className="section-title">{`Tracked applications (${filteredApplicationsByTeam.length})`}
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
                <div className="apps-table">
                  <div> </div>
                  {(Environments).map(env => (
                    <div className="env-header">

                      <button
                        key={env}
                        className={`env-btn ${selectedEnv === env ? "active" : ""}`}
                        onClick={() => setSelectedEnv(env)}
                      >
                        {env.toUpperCase()}
                      </button>

                    </div>
                  ))}
                  {higherFilteredApplications.map((app, index) => (
                    <>
                      {/* APPLICATION BUTTON */}
                      <button
                        //  Todo display tooltip on hover with  error messages

                        disabled={!isApplicationExistInAnyEnv(app.name)
                        }
                        key={app.name}
                        className="app-card slide-up"
                        style={{ animationDelay: `${index * 60}ms` }}
                        onClick={() => selectAppInAnyEnv(app.name)}

                      >
                        {app.name}
                      </button>

                      {/* BALLS */}
                      {Environments.map(env => (
                        <div className="status-cell">
                          <button
                            disabled={
                              isDisabled(env, app.name)
                            }
                            className="env-btn mini"
                            onClick={() => {
                              selectAppInEnv(env, app.name);
                            }}>
                            {getBall(env, app.name)}
                          </button>
                        </div>
                      ))}
                    </>
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
                    {/* change */}

                    <button className="critical-filter-btn" style={{ background: isFilteredByCritical ? "grey" : "#020617", marginLeft: "8px" }} onClick={() => setIsFilteredByCritical(!isFilteredByCritical)} >  🔴 </button>
                  </span>
                </div>
                <select style={{ position: "fixed", top: "68px", width: "468px" }}
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

                    className={`sidebar-item ${selectedApp === app ? "active" : ""}`}

                    onClick={() =>
                      selectAppInAnyEnv(app.name)}
                  >
                    {/* // show balls of envs in onlift */}
                    <span className="sidebar-icon">
                      {Environments.map(env => getBall(env, app.name))}
                    </span>
                    <span className="sidebar-text">
                      {app.name}
                    </span>
                  </div>
                ))}
              </div>
            </aside>
            {selectedApp && applicationDetails[selectedEnv] ? (

              <main className="main"  >
                <ContractComparisonResult {...applicationDetails[selectedEnv]!} env={selectedEnv} onEnvChange={changeEnvironment} getColorOfTab={getColorOfTab} isDisabled={isDisabled} />
              </main>

            ) : (
              <div className="main placeholder" style={{ paddingTop: "150px" }}>
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

