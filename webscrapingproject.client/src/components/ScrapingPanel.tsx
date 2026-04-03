import React, { useState, useEffect } from 'react';
import * as signalR from '@microsoft/signalr';

const ScrapingPanel = () => {
    const [progress, setProgress] = useState(0);
    const [message, setMessage] = useState("Sistem Hazır");
    const [isScraping, setIsScraping] = useState(false);


    useEffect(() => {
       
        const checkStatus = async () => {
            try {
                const response = await fetch('https://localhost:7078/api/scraping/status');
                const data = await response.json();
                
                if (data.isScraping) {
                    setIsScraping(true);
                    setMessage("Başlangıçtaki otomatik tarama devam ediyor...");

                }
            } catch (error) {
                console.error("Durum kontrolü başarısız:", error);
            }
        };

        checkStatus();

      
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("https://localhost:7078/scrapingHub")
            .withAutomaticReconnect()
            .build();

        connection.on("ReceiveProgress", (percent, currentMessage) => {
          
            setIsScraping(true); 
            setProgress(percent);
            setMessage(currentMessage);

           
            if (percent === 100) {
                setTimeout(() => {
                    setIsScraping(false);
                    setMessage("Sistem Hazır");
                    setProgress(0);
                }, 3000); 
            }
        });

        connection.start()
            .then(() => console.log("SignalR Bağlantısı Başarılı!"))
            .catch(err => console.error("SignalR Hatası: ", err));

        return () => {
            connection.stop();
        };
    }, []);

    const handleStartScraping = async () => {
        if (isScraping) return;

        setIsScraping(true);
        setProgress(0);
        setMessage("Tarama başlatılıyor...");

        try {
            const response = await fetch('https://localhost:7078/api/scraping/start', {
                method: 'POST'
            });

            if (!response.ok) {
                const errorData = await response.json();
                setMessage(errorData.message || "Başlatılamadı!");
                setIsScraping(false);
            }
        } catch (error) {
            console.error("Başlatma hatası", error);
            setIsScraping(false);
            setMessage("Sunucuya ulaşılamadı!");
        }
    };

    return (
        <div style={{ padding: '20px', border: '1px solid #ccc', borderRadius: '8px', marginBottom: '20px', backgroundColor: '#f9f9f9' }}>
            
            
            <button 
                onClick={handleStartScraping} 
                disabled={isScraping}
                style={{ 
                    padding: '10px 20px', 
                    cursor: isScraping ? 'not-allowed' : 'pointer',
                    backgroundColor: isScraping ? '#9e9e9e' : '#2196f3',
                    color: 'white',
                    border: 'none',
                    borderRadius: '4px',
                    fontWeight: 'bold'
                }}
            >
                {isScraping ? 'Zaten haberler çekiliyor' : 'Tüm Haberleri Yeniden Çek'}
            </button>

        
            {isScraping && (
                <div style={{ marginTop: '20px' }}>
                    <p style={{ margin: '0 0 5px 0', fontSize: '14px', color: '#555' }}><strong>Durum:</strong> {message}</p>
                    <div style={{ width: '100%', backgroundColor: '#e0e0e0', borderRadius: '5px', overflow: 'hidden' }}>
                        <div 
                            style={{ 
                                width: `${progress}%`, 
                                height: '24px', 
                                backgroundColor: progress === 100 ? '#4caf50' : '#ff9800', 
                                transition: 'width 0.5s ease-in-out',
                                textAlign: 'center',
                                color: 'white',
                                lineHeight: '24px',
                                fontWeight: 'bold'
                            }}
                        >
                            {progress > 0 ? `${progress}%` : ''}
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default ScrapingPanel;