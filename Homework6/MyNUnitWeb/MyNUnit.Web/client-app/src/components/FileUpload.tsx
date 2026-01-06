import React, { useState } from 'react';
import axios from 'axios';

const FileUpload: React.FC = () => {
    const [files, setFiles] = useState<FileList | null>(null);
    const [uploading, setUploading] = useState(false);
    const [message, setMessage] = useState('');

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setFiles(e.target.files);
    };

    const handleUpload = async () => {
        if (!files || files.length === 0) {
            setMessage('Please select files first');
            return;
        }

        setUploading(true);
        setMessage('');

        const formData = new FormData();
        for (let i = 0; i < files.length; i++) {
            formData.append('files', files[i]);
        }

        try {
            const response = await axios.post('http://localhost:5284/api/tests/upload', formData, {
                headers: {
                    'Content-Type': 'multipart/form-data',
                },
            });

            setMessage(`Successfully uploaded ${response.data.files?.length || 0} file(s)`);
            setFiles(null);

            // Clear file input
            const fileInput = document.querySelector('input[type="file"]') as HTMLInputElement;
            if (fileInput) fileInput.value = '';

        } catch (error: any) {
            setMessage(`Error: ${error.response?.data?.error || error.message}`);
        } finally {
            setUploading(false);
        }
    };

    return (
        <div className="card">
            <div className="card-body">
                <h5 className="card-title">Upload Test Assemblies</h5>
                <p className="card-text">Select DLL files containing test classes.</p>

                <div className="mb-3">
                    <input
                        type="file"
                        className="form-control"
                        multiple
                        accept=".dll"
                        onChange={handleFileChange}
                    />
                    <div className="form-text">
                        Select one or more DLL files with test classes
                    </div>
                </div>

                {files && files.length > 0 && (
                    <div className="mb-3">
                        <strong>Selected files:</strong>
                        <ul className="mb-0">
                            {Array.from(files).map((file, index) => (
                                <li key={index}>{file.name}</li>
                            ))}
                        </ul>
                    </div>
                )}

                <button
                    className="btn btn-primary"
                    onClick={handleUpload}
                    disabled={uploading || !files || files.length === 0}
                >
                    {uploading ? 'Uploading...' : 'Upload Files'}
                </button>

                {message && (
                    <div className={`alert ${message.includes('Error') ? 'alert-danger' : 'alert-success'} mt-3`}>
                        {message}
                    </div>
                )}
            </div>
        </div>
    );
};

export default FileUpload;