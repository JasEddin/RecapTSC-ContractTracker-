import type { ApplicationDetails } from "../App";

export function ContractComparisonResult( applicationDetails: ApplicationDetails ) {
 const breakingChanges = applicationDetails.changes.filter(change => change.changeType === 1);
 const informationalChanges = applicationDetails.changes.filter(change => change.changeType === 0);
  return (
       <div className="result-card">
      <h2>{applicationDetails.name} - Contract Comparison</h2>
      {breakingChanges.length > 0 && (
        <div className="breaking-changes">
          <h3>Breaking Changes</h3>
          <ul>
            {breakingChanges.map((change, index) => (
              <li key={index}>{change.message}</li>
            ))}
          </ul>
        </div>
      )}
      {
      informationalChanges.length > 0 &&
        ( <div className="informational-changes">
          <h3>Informational Changes</h3>
          <ul>
            {informationalChanges.map((change, index) => (
              <li key={index}>{change.message}</li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
}