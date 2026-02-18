import type { ApplicationDetails } from "../App";


export function ContractComparisonResult({ changes, name, url, localContractPath }: ApplicationDetails) { 
 const breakingChanges = changes.filter(change => change.impact === 1);
 const informationalChanges = changes.filter(change => change.impact === 0);
  return (
       <div className="result-card">
      <h2>{name} – Contract Comparison</h2>
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