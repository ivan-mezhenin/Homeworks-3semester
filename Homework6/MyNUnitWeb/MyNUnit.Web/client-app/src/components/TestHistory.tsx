import React, { useState, useEffect } from 'react';
import axios from 'axios';

interface TestRun {
    id: string;
    runTime: string;
    totalTests: number;
    passed: number;
    failed: number;
    ignored: number;
    error: number;
    totalDurationMs: number;
}

const TestHistory: React.FC = () => {
    const [testRuns, setTestRuns] = useState<TestRun[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    useEffect(() => {
        fetchTestRuns();
    }, []);

    const fetchTestRuns = async () => {
        try {
            const response = await axios.get('http://localhost:5284/api/tests/runs');
            setTestRuns(response.data);
        } catch (err: any) {
            setError('Failed to load test history');
        } finally {
            setLoading(false);
        }
    };

    if (loading) {
        return (
            <div className="d-flex justify-content-center">
                <div className="spinner-border" role="status">
                    <span className="visually-hidden">Loading...</span>
                </div>
            </div>
        );
    }

    return (
        <div className="card">
            <div className="card-body">
                <h5 className="card-title">Test History</h5>

                {error && (
                    <div className="alert alert-danger">{error}</div>
                )}

                {testRuns.length === 0 ? (
                    <p>No test runs found. Upload some assemblies and run tests to see history.</p>
                ) : (
                    <div className="table-responsive">
                        <table className="table table-striped">
                            <thead>
                            <tr>
                                <th>Run Time</th>
                                <th>Tests</th>
                                <th>Results</th>
                                <th>Duration</th>
                            </tr>
                            </thead>
                            <tbody>
                            {testRuns.map(run => (
                                <tr key={run.id}>
                                    <td>{new Date(run.runTime).toLocaleString()}</td>
                                    <td>{run.totalTests}</td>
                                    <td>
                                        <span className="badge bg-success me-1">{run.passed} Passed</span>
                                        <span className="badge bg-danger me-1">{run.failed} Failed</span>
                                        <span className="badge bg-warning me-1">{run.ignored} Ignored</span>
                                        <span className="badge bg-secondary">{run.error} Errors</span>
                                    </td>
                                    <td>{(run.totalDurationMs / 1000).toFixed(2)}s</td>
                                </tr>
                            ))}
                            </tbody>
                        </table>
                    </div>
                )}
            </div>
        </div>
    );
};

export default TestHistory;