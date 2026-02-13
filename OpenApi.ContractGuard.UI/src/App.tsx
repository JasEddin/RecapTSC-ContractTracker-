import Logo from "./assets/logo.svg";
import "./App.css";

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

      <div className="app-list">
        {applications.map((app, index) => (
          <div
            key={app}
            className="app-card slide-up"
            style={{ animationDelay: `${index * 60}ms` }}
          >
            {app}
          </div>
        ))}
      </div>
    </section>
  </div>
</div>
  );
}

export default App;