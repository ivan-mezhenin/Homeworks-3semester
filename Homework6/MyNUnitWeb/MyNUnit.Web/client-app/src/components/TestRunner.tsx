import React, { useState, useEffect } from 'react';
import axios from 'axios';

interface Assembly {
    id: string;
    fileName: string;
    uploadTime: string;
    testRunId?: string;
}

const TestRunner: React.FC = () => {
    const [assemblies, setAssemblies] = useState<Assembly[]>([]);
    const [selectedAssemblies, setSelectedAssemblies] = useState<string[]>([]);
    const [running, setRunning] = useState(false);
    const [result, setResult] = useState<any>(null);
    const [message, setMessage] = useState('');

    useEffect(() => {
        fetchAssemblies();
    }, []);

    const fetchAssemblies = async () => {
        try {
            const response = await axios.get('http://localhost:5284/api/tests/assemblies');
            setAssemblies(response.data);
        } catch (error) {
            console.error('Failed to fetch assemblies:', error);
            setMessage('Failed to load assemblies');
        }
    };

    const handleAssemblyToggle = (id: string) => {
        setSelectedAssemblies(prev =>
            prev.includes(id)
                ? prev.filter(assemblyId => assemblyId !== id)
                : [...prev, id]
        );
    };

    const handleRunTests = async () => {
        if (selectedAssemblies.length === 0) {
            setMessage('Please select at least one assembly to test');
            return;
        }

        setRunning(true);
        setMessage('');
        setResult(null);

        try {
            const response = await axios.post('http://localhost:5284/api/tests/run', {
                assemblyIds: selectedAssemblies
            });

            setResult(response.data);
            setMessage(`Tests completed: ${response.data.summary.total} tests executed`);

            // Refresh assemblies to update testRunId
            await fetchAssemblies();
        } catch (error: any) {
            setMessage(`Error: ${error.response?.data?.error || error.message}`);
        } finally {
            setRunning(false);
        }
    };

    const selectAll = () => {
        setSelectedAssemblies(assemblies.map(a => a.id));
    };

    const clearSelection = () => {
        setSelectedAssemblies([]);
    };

    return (
        <div className="card">
            <div className="card-body">
                <h5 className="card-title">Run Tests</h5>

                {message && (
                    <div className={`alert ${message.includes('Error') ? 'alert-danger' : 'alert-success'} mb-3`}>
                        {message}
                    </div>
                )}

                <div className="mb-4">
                    <div className="d-flex justify-content-between align-items-center mb-3">
                        <h6>Select Assemblies to Test</h6>
                        <div>
                            <button className="btn btn-sm btn-outline-secondary me-2" onClick={selectAll}>
                                Select All
                            </button>
                            <button className="btn btn-sm btn-outline-secondary" onClick={clearSelection}>
                                Clear
                            </button>
                        </div>
                    </div>

                    {assemblies.length === 0 ? (
                        <div className="alert alert-info">
                            No assemblies uploaded yet. Please upload DLL files first.
                        </div>
                    ) : (
                        <div className="list-group">
                            {assemblies.map(assembly => (
                                <div
                                    key={assembly.id}
                                    className={`list-group-item list-group-item-action ${selectedAssemblies.includes(assembly.id) ? 'active' : ''}`}
                                    onClick={() => handleAssemblyToggle(assembly.id)}
                                    style={{ cursor: 'pointer' }}
                                >
                                    <div className="d-flex w-100 justify-content-between align-items-center">
                                        <div className="form-check">
                                            <input
                                                className="form-check-input"
                                                type="checkbox"
                                                checked={selectedAssemblies.includes(assembly.id)}
                                                onChange={() => {}}
                                                onClick={(e) => e.stopPropagation()}
                                            />
                                            <label className="form-check-label ms-2">
                                                {assembly.fileName}
                                            </label>
                                        </div>
                                        <div>
                                            <small className="text-muted">
                                                {new Date(assembly.uploadTime).toLocaleDateString()}
                                            </small>
                                            {assembly.testRunId && (
                                                <span className="badge bg-success ms-2">Tested</span>
                                            )}
                                        </div>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </div>

                <div className="d-flex justify-content-between align-items-center">
                    <button
                        className="btn btn-primary"
                        onClick={handleRunTests}
                        disabled={running || selectedAssemblies.length === 0}
                    >
                        {running ? (
                            <>
                                <span className="spinner-border spinner-border-sm me-2" role="status"></span>
                                Running Tests...
                            </>
                        ) : (
                            'Run Tests'
                        )}
                    </button>
                    <span className="text-muted">
                        Selected: {selectedAssemblies.length} of {assemblies.length}
                    </span>
                </div>

                {result && (
                    <div className="mt-4">
                        <h6>Test Results</h6>
                        <div className="card">
                            <div className="card-body">
                                <div className="row">
                                    <div className="col-md-3 text-center">
                                        <div className={`display-6 ${result.summary.failed + result.summary.errors > 0 ? 'text-danger' : 'text-success'}`}>
                                            {result.summary.passed}/{result.summary.total}
                                        </div>
                                        <div>Tests Passed</div>
                                    </div>
                                    <div className="col-md-9">
                                        <div className="row">
                                            <div className="col-3">
                                                <div className="text-success">✓ {result.summary.passed}</div>
                                            </div>
                                            <div className="col-3">
                                                <div className="text-danger">✗ {result.summary.failed}</div>
                                            </div>
                                            <div className="col-3">
                                                <div className="text-warning">~ {result.summary.ignored}</div>
                                            </div>
                                            <div className="col-3">
                                                <div className="text-danger">! {result.summary.errors}</div>
                                            </div>
                                        </div>
                                        <div className="mt-2">
                                            <small>Duration: {result.summary.duration}ms</small>
                                            <br />
                                            <small>Run ID: {result.runId}</small>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
};

export default TestRunner;