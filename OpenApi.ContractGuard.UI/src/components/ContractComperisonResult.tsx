type Change = {
  type: "info" | "breaking";
  message: string;
};

type Props = {
  appName: string;
  beforeVersion: string;
  afterVersion: string;
  informationalChanges: Change[];
  breakingChanges: Change[];
};

export function ContractComparisonResult({
  appName,
  beforeVersion,
  afterVersion,
  informationalChanges,
  breakingChanges,
}: Props) {
  return (
    <div className="result-card">
      <h2>{appName} – Contract comparison</h2>

      <div className="version-row">
        <span className="version before">Before: {beforeVersion}</span>
        <span className="arrow">→</span>
        <span className="version after">After: {afterVersion}</span>
      </div>

      {/* Breaking changes */}
      <section className="changes-section">
        <h3 className="breaking-title">⚠ Requires contract update</h3>

        {breakingChanges.length === 0 ? (
          <p className="empty">No breaking changes 🎉</p>
        ) : (
          <ul>
            {breakingChanges.map((c, i) => (
              <li key={i} className="breaking-item">
                {c.message}
              </li>
            ))}
          </ul>
        )}
      </section>

      {/* Informational changes */}
      <section className="changes-section">
        <h3 className="info-title">ℹ Informational changes</h3>

        {informationalChanges.length === 0 ? (
          <p className="empty">No informational changes</p>
        ) : (
          <ul>
            {informationalChanges.map((c, i) => (
              <li key={i} className="info-item">
                {c.message}
              </li>
            ))}
          </ul>
        )}
      </section>
    </div>
  );
}