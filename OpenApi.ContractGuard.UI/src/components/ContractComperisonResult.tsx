import { useState } from "react";
import { Environments, type ApplicationDetails, type Environment } from "../App";
import swaggerIcon from "../assets/swagger-icon.svg";

export function ContractComparisonResult({
  changes,
  name,
  server,
  localContractPath,
  env,
  onEnvChange, 
  getColorOfTab,
  isDisabled
}: ApplicationDetails & { env: Environment; onEnvChange: (env: Environment) => void, getColorOfTab: (env: Environment, appName: string) => string, isDisabled: (env: Environment, appName: string) => boolean }) {

  const [selectedEnv, setSelectedEnv] = useState<Environment>(env);
  const breakingChanges = changes.filter(change => change.impact === 1);
  const informationalChanges = changes.filter(change => change.impact === 0);
  const serverUrl = server.replace("$(environment)", `.${selectedEnv}.`);
  return (
    <div className="result-card">

      <h2>{name} – Contract Comparison</h2>

      {/* ENVIRONMENT TABS */}
      <div className="env-tabs">
        {Environments.map(env => (
          <button
            disabled={isDisabled(env, name)}
            key={env}
            className={`env-tab ${selectedEnv === env ? "active" : ""}`}
            onClick={() => {
              setSelectedEnv(env);
              onEnvChange(env);
            }}
            style={{ backgroundColor: getColorOfTab(env, name) }}
          >
            {env.toUpperCase()}
          </button>
        ))}
      </div>

      <div className="floating-actions">
        <button
          className="floating-btn swagger-btn"
          onClick={() =>
            window.open(`${serverUrl}/swagger/index.html`, "_blank")
          }
        >
          <img src={swaggerIcon} alt="Swagger" className="swagger-icon" />
          <span>Swagger</span>
        </button>

        <button
          className="floating-btn local-btn"
          onClick={() =>
            window.open(
              `http://localhost:5093/api/open-with-vscode?path=${encodeURIComponent(
                localContractPath
              )}`,
              "_blank"
            )
          }
        >
          📂 View Contract in Apis
        </button>
      </div>

      {/* Breaking changes */}
      <div className="changes-section">
        <h3 className="breaking-title">
          🚨 Breaking Changes ({breakingChanges.length})
        </h3>

        {breakingChanges.length > 0 ? (
          <ul className="changes-list breaking-list">
            {breakingChanges.map((change, index) => (
              <li key={index} className="breaking-item">
                {change.message}
              </li>
            ))}
          </ul>
        ) : (
          <p className="empty">No breaking changes 🎉</p>
        )}
      </div>

      {/* Informational changes */}
      <div className="changes-section">
        <h3 className="info-title">
          ℹ️ Informational Changes ({informationalChanges.length})
        </h3>

        {informationalChanges.length > 0 ? (
          <ul className="changes-list info-list">
            {informationalChanges.map((change, index) => (
              <li key={index} className="info-item">
                {change.message}
              </li>
            ))}
          </ul>
        ) : (
          <p className="empty">No informational changes</p>
        )}
      </div>
    </div>
  );
}