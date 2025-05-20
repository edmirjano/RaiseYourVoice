import React, { useEffect, useRef, useState } from 'react';
import { useTranslation } from 'next-i18next';
import { Post } from '../../services/postService';
import { Alert } from '../common/Alert/Alert';
import { Loader } from '../common/Loader/Loader';

interface MapViewProps {
  opportunities: Post[];
  className?: string;
  height?: string;
}

export const MapView: React.FC<MapViewProps> = ({
  opportunities,
  className = '',
  height = '600px',
}) => {
  const { t } = useTranslation('common');
  const mapRef = useRef<HTMLDivElement>(null);
  const [mapLoaded, setMapLoaded] = useState(false);
  const [mapError, setMapError] = useState('');
  const [map, setMap] = useState<google.maps.Map | null>(null);
  const [markers, setMarkers] = useState<google.maps.Marker[]>([]);
  const [infoWindow, setInfoWindow] = useState<google.maps.InfoWindow | null>(null);
  
  // Load Google Maps script
  useEffect(() => {
    // Check if Google Maps is already loaded
    if (window.google && window.google.maps) {
      setMapLoaded(true);
      return;
    }
    
    // Create script element
    const script = document.createElement('script');
    script.src = `https://maps.googleapis.com/maps/api/js?key=${process.env.NEXT_PUBLIC_GOOGLE_MAPS_API_KEY}&libraries=places`;
    script.async = true;
    script.defer = true;
    
    // Handle script load success
    script.onload = () => {
      setMapLoaded(true);
    };
    
    // Handle script load error
    script.onerror = () => {
      setMapError(t('opportunities.mapLoadError'));
    };
    
    // Add script to document
    document.head.appendChild(script);
    
    // Cleanup
    return () => {
      // Remove script if it hasn't loaded yet
      if (!script.onload) {
        document.head.removeChild(script);
      }
    };
  }, [t]);
  
  // Initialize map when script is loaded
  useEffect(() => {
    if (!mapLoaded || !mapRef.current) return;
    
    try {
      // Create map instance
      const mapInstance = new google.maps.Map(mapRef.current, {
        center: { lat: 41.3275, lng: 19.8187 }, // Default center (Tirana, Albania)
        zoom: 5,
        mapTypeControl: true,
        streetViewControl: false,
        fullscreenControl: true,
        zoomControl: true,
      });
      
      // Create info window
      const infoWindowInstance = new google.maps.InfoWindow();
      
      setMap(mapInstance);
      setInfoWindow(infoWindowInstance);
    } catch (error) {
      console.error('Error initializing map:', error);
      setMapError(t('opportunities.mapInitError'));
    }
  }, [mapLoaded, t]);
  
  // Add markers for opportunities
  useEffect(() => {
    if (!map || !infoWindow) return;
    
    // Clear existing markers
    markers.forEach(marker => marker.setMap(null));
    
    // Filter opportunities with valid location data
    const validOpportunities = opportunities.filter(
      opp => opp.location?.latitude && opp.location?.longitude
    );
    
    if (validOpportunities.length === 0) {
      // If no valid locations, don't add any markers
      setMarkers([]);
      return;
    }
    
    // Create bounds to fit all markers
    const bounds = new google.maps.LatLngBounds();
    
    // Create new markers
    const newMarkers = validOpportunities.map(opportunity => {
      const position = {
        lat: opportunity.location!.latitude!,
        lng: opportunity.location!.longitude!
      };
      
      // Extend bounds
      bounds.extend(position);
      
      // Create marker
      const marker = new google.maps.Marker({
        position,
        map,
        title: opportunity.title,
        animation: google.maps.Animation.DROP,
      });
      
      // Add click listener to show info window
      marker.addListener('click', () => {
        // Create info window content
        const content = `
          <div class="p-2">
            <h3 class="font-semibold text-lg">${opportunity.title}</h3>
            <p class="text-sm text-gray-600 mt-1">${opportunity.location?.city}, ${opportunity.location?.country}</p>
            ${opportunity.eventDate ? `
              <p class="text-sm text-gray-600 mt-1">
                ${new Date(opportunity.eventDate).toLocaleDateString()}
              </p>
            ` : ''}
            <a href="/opportunities/${opportunity.id}" class="text-blue-600 hover:underline text-sm mt-2 inline-block">
              ${t('opportunities.viewDetails')}
            </a>
          </div>
        `;
        
        infoWindow.setContent(content);
        infoWindow.open(map, marker);
      });
      
      return marker;
    });
    
    // Set new markers
    setMarkers(newMarkers);
    
    // Fit map to bounds
    map.fitBounds(bounds);
    
    // If only one marker, zoom out a bit
    if (newMarkers.length === 1) {
      map.setZoom(12);
    }
    
    // Cleanup
    return () => {
      newMarkers.forEach(marker => marker.setMap(null));
    };
  }, [map, infoWindow, opportunities, t]);
  
  if (mapError) {
    return (
      <Alert variant="error" className={className}>
        {mapError}
      </Alert>
    );
  }
  
  return (
    <div className={`relative rounded-lg overflow-hidden ${className}`} style={{ height }}>
      {!mapLoaded && (
        <div className="absolute inset-0 flex items-center justify-center bg-gray-100">
          <Loader size="lg" type="spinner" text={t('opportunities.loadingMap')} />
        </div>
      )}
      <div ref={mapRef} className="w-full h-full" />
    </div>
  );
};