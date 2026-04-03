import { useState, useEffect, useRef } from 'react';
import { GoogleMap, LoadScript, OverlayView, InfoWindow } from '@react-google-maps/api';
import axios from 'axios';
import './App.css';
import ScrapingPanel from './components/ScrapingPanel';

interface NewsArticle {
    id: string;
    title: string;
    content: string;
    category: string;
    locationText: string;
    latitude: number;
    longitude: number;
    publishDate: string;
    sourceNames: string[];
    url: string;
}

const containerStyle = {
    width: '100%',
    height: '100vh'
};

const defaultCenter = {
    lat: 40.7654,
    lng: 29.9408
};

const GOOGLE_MAPS_API_KEY = import.meta.env.VITE_GOOGLE_MAPS_API_KEY;

const kocaeliDistricts = [
    "Tümü", "İzmit", "Gebze", "Gölcük", "Kandıra", "Karamürsel", "Körfez",
    "Derince", "Kartepe", "Başiskele", "Çayırova", "Dilovası", "Darıca"
];

function App() {
    const [newsList, setNewsList] = useState<NewsArticle[]>([]);
    const [selectedArticle, setSelectedArticle] = useState<NewsArticle | null>(null);

    const [categoryFilter, setCategoryFilter] = useState("Tümü");
    const [districtFilter, setDistrictFilter] = useState("Tümü");
    const [startDate, setStartDate] = useState("");
    const [endDate, setEndDate] = useState("");

    const mapRef = useRef<google.maps.Map | null>(null);

    const getCategoryStyle = (category: string) => {
        switch (category) {
            case "Trafik Kazası": return { color: "#e74c3c", icon: "trafikkazası" };
            case "Yangın": return { color: "#e67e22", icon: "ateş" };
            case "Elektrik Kesintisi": return { color: "#f1c40f", icon: "elektrik" };
            case "Hırsızlık": return { color: "#8e44ad", icon: "hırsızlık" };
            case "Kültürel Etkinlikler": return { color: "#2ecc71", icon: "cultural" };
            default: return { color: "#3498db", icon: "cultural" };
        }
    };

    const fetchNews = async () => {
        try {
            let url = `https://localhost:7078/api/news?`;

            if (categoryFilter !== "Tümü") url += `category=${categoryFilter}&`;
            if (startDate) url += `startDate=${startDate}&`;
            if (endDate) url += `endDate=${endDate}&`;

            const response = await axios.get(url);
            setNewsList(response.data);
        } catch (error) {
            console.error("Haberler çekilirken hata oluştu:", error);
        }
    };

    useEffect(() => {
        if (!GOOGLE_MAPS_API_KEY) return;
        fetchNews();
    }, [categoryFilter, startDate, endDate]);

    const filteredNews = newsList.filter(news => {
        if (districtFilter === "Tümü") return true;
        return news.locationText.includes(districtFilter);
    });

    const handleListItemClick = (article: NewsArticle) => {
        setSelectedArticle(article);

        if (mapRef.current) {
            mapRef.current.panTo({ lat: article.latitude, lng: article.longitude });
            mapRef.current.setZoom(15);
        }
    };

    return (
        <div style={{ display: 'flex', height: '100vh', width: '100vw', overflow: 'hidden' }}>

            
            <div className="sidebar">

                
                <div className="sidebar-header">
                    <h2 className="sidebar-title">Kocaeli Haber Haritası</h2>

                    <ScrapingPanel />

                    <div className="filter-group">
                        <label className="filter-label">İlçe Seçimi</label>
                        <select
                            className="filter-input"
                            value={districtFilter}
                            onChange={(e) => {
                                setDistrictFilter(e.target.value);
                                setSelectedArticle(null);
                            }}
                        >
                            {kocaeliDistricts.map(district => (
                                <option key={district} value={district}>{district}</option>
                            ))}
                        </select>
                    </div>

                    <div className="filter-group">
                        <label className="filter-label">Olay Türü</label>
                        <select
                            className="filter-input"
                            value={categoryFilter}
                            onChange={(e) => {
                                setCategoryFilter(e.target.value);
                                setSelectedArticle(null);
                            }}
                        >
                            <option value="Tümü">Tümü</option>
                            <option value="Trafik Kazası">Trafik Kazası</option>
                            <option value="Yangın">Yangın</option>
                            <option value="Elektrik Kesintisi">Elektrik Kesintisi</option>
                            <option value="Hırsızlık">Hırsızlık</option>
                            <option value="Kültürel Etkinlikler">Kültürel Etkinlikler</option>
                        </select>
                    </div>

                    <div style={{ display: 'flex', gap: '10px' }}>
                        <div className="filter-group" style={{ flex: 1, marginBottom: 0 }}>
                            <label className="filter-label">Başlangıç</label>
                            <input
                                className="filter-input"
                                type="date"
                                value={startDate}
                                onChange={(e) => setStartDate(e.target.value)}
                            />
                        </div>
                        <div className="filter-group" style={{ flex: 1, marginBottom: 0 }}>
                            <label className="filter-label">Bitiş</label>
                            <input
                                className="filter-input"
                                type="date"
                                value={endDate}
                                onChange={(e) => setEndDate(e.target.value)}
                            />
                        </div>
                    </div>
                </div>

                
                <div className="news-list-container">
                    <div className="news-list-header">
                        <h3 className="news-list-title">Haritadaki Olaylar</h3>
                        <span className="news-count">{filteredNews.length}</span>
                    </div>

                    <ul style={{ listStyleType: 'none', padding: 0 }}>
                        {filteredNews.map(news => (
                            <li
                                key={news.id}
                                className="news-card"
                                onClick={() => handleListItemClick(news)}
                                style={{ borderLeft: `5px solid ${getCategoryStyle(news.category).color}` }}
                            >
                                <div className="news-card-title">{news.title}</div>
                                <div className="news-card-meta">
                                    <span>📅 {new Date(news.publishDate).toLocaleDateString()}</span>
                                    <span>📍 {news.locationText}</span>
                                </div>
                            </li>
                        ))}
                    </ul>
                </div>
            </div>

            
            <div style={{ flex: 1, position: 'relative' }}>
                {GOOGLE_MAPS_API_KEY ? (
                    <LoadScript googleMapsApiKey={GOOGLE_MAPS_API_KEY}>
                        <GoogleMap
                            mapContainerStyle={containerStyle}
                            center={defaultCenter}
                            zoom={11}
                            onLoad={(map) => { mapRef.current = map; }}
                        >
                            {filteredNews.map((article) => {
                                const styleInfo = getCategoryStyle(article.category);
                                return (
                                    <OverlayView
                                        key={article.id}
                                        position={{ lat: article.latitude, lng: article.longitude }}
                                        mapPaneName={OverlayView.OVERLAY_MOUSE_TARGET}
                                    >
                                        <div
                                            onClick={() => setSelectedArticle(article)}
                                            style={{
                                                width: '45px',
                                                height: '45px',
                                                backgroundColor: styleInfo.color,
                                                borderRadius: '50%',
                                                display: 'flex',
                                                alignItems: 'center',
                                                justifyContent: 'center',
                                                border: '3px solid white',
                                                boxShadow: '0 4px 8px rgba(0,0,0,0.4)',
                                                cursor: 'pointer',
                                                transform: 'translate(-50%, -50%)',
                                                transition: 'transform 0.2s',
                                            }}
                                            onMouseEnter={(e) => e.currentTarget.style.transform = 'translate(-50%, -50%) scale(1.1)'}
                                            onMouseLeave={(e) => e.currentTarget.style.transform = 'translate(-50%, -50%) scale(1)'}
                                        >
                                            <img
                                                src={`/${styleInfo.icon}.png`}
                                                alt={article.category}
                                                style={{ width: '25px', height: '25px', objectFit: 'contain' }}
                                            />
                                        </div>
                                    </OverlayView>
                                );
                            })}

                            {selectedArticle && (
                                <InfoWindow
                                    position={{ lat: selectedArticle.latitude, lng: selectedArticle.longitude }}
                                    onCloseClick={() => setSelectedArticle(null)}
                                >
                                    <div style={{ maxWidth: '250px', padding: '5px' }}>
                                        <h4 style={{ margin: '0 0 10px 0', color: '#1a202c' }}>{selectedArticle.title}</h4>
                                        <p style={{ margin: '0 0 5px 0', fontSize: '12px', color: '#718096' }}><strong>Tarih:</strong> {new Date(selectedArticle.publishDate).toLocaleDateString()}</p>
                                        <p style={{ margin: '0 0 10px 0', fontSize: '12px', color: '#718096' }}><strong>Kaynak:</strong> {selectedArticle.sourceNames.join(", ")}</p>
                                        <a
                                            href={selectedArticle.url}
                                            target="_blank"
                                            rel="noopener noreferrer"
                                            style={{ display: 'inline-block', padding: '6px 12px', backgroundColor: '#3182ce', color: 'white', textDecoration: 'none', borderRadius: '6px', fontSize: '12px', textAlign: 'center', fontWeight: 'bold', marginTop: '5px' }}
                                        >
                                            Habere Git
                                        </a>
                                    </div>
                                </InfoWindow>
                            )}
                        </GoogleMap>
                    </LoadScript>
                ) : (
                    <div style={{ display: 'flex', height: '100%', alignItems: 'center', justifyContent: 'center', backgroundColor: '#f0f0f0', color: 'red' }}>
                        <h3>google maps api key eksik</h3>
                    </div>
                )}
            </div>
        </div>
    );
}

export default App;