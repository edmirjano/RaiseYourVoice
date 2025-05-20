import React, { useState, useEffect, useRef } from 'react';
import { useTranslation } from 'next-i18next';
import { Input } from '../common/Form/Input';

interface LocationSearchProps {
  initialValue?: string;
  onSelect: (location: {
    address: string;
    city: string;
    country: string;
    latitude: number;
    longitude: number;
  }) => void;
  placeholder?: string;
  className?: string;
}

export const LocationSearch: React.FC<LocationSearchProps> = ({
  initialValue = '',
  onSelect,
  placeholder,
  className = '',
}) => {
  const { t } = useTranslation('common');
  const [inputValue, setInputValue] = useState(initialValue);
  const [isLoaded, setIsLoaded] = useState(false);
  const [error, setError] = useState('');
  const autocompleteRef = useRef<google.maps.places.Autocomplete | null>(null);
  const inputRef = useRef<HTMLInputElement>(null);
  
  // Load Google Maps Places API
  useEffect(() => {
    // Check if Google Maps is already loaded
    if (window.google && window.google.maps && window.google.maps.places) {
      setIsLoaded(true);
      return;
    }
    
    // Create script element
    const script = document.createElement('script');
    script.src = `https://maps.googleapis.com/maps/api/js?key=${process.env.NEXT_PUBLIC_GOOGLE_MAPS_API_KEY}&libraries=places`;
    script.async = true;
    script.defer = true;
    
    // Handle script load success
    script.onload = () => {
      setIsLoaded(true);
    };
    
    // Handle script load error
    script.onerror = () => {
      setError(t('opportunities.mapLoadError'));
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
  
  // Initialize autocomplete when script is loaded
  useEffect(() => {
    if (!isLoaded || !inputRef.current) return;
    
    try {
      // Create autocomplete instance
      const autocomplete = new google.maps.places.Autocomplete(inputRef.current, {
        types: ['geocode'],
      });
      
      // Store reference
      autocompleteRef.current = autocomplete;
      
      // Add place_changed event listener
      autocomplete.addListener('place_changed', () => {
        const place = autocomplete.getPlace();
        
        if (!place.geometry || !place.geometry.location) {
          setError(t('opportunities.invalidLocation'));
          return;
        }
        
        // Extract address components
        let city = '';
        let country = '';
        
        place.address_components?.forEach(component => {
          if (component.types.includes('locality')) {
            city = component.long_name;
          } else if (component.types.includes('country')) {
            country = component.long_name;
          }
        });
        
        // Call onSelect with location data
        onSelect({
          address: place.formatted_address || '',
          city,
          country,
          latitude: place.geometry.location.lat(),
          longitude: place.geometry.location.lng(),
        });
        
        // Update input value
        setInputValue(place.formatted_address || '');
      });
    } catch (error) {
      console.error('Error initializing autocomplete:', error);
      setError(t('opportunities.autocompleteError'));
    }
  }, [isLoaded, onSelect, t]);
  
  return (
    <div className={className}>
      <Input
        ref={inputRef}
        type="text"
        value={inputValue}
        onChange={(e) => setInputValue(e.target.value)}
        placeholder={placeholder || t('opportunities.locationPlaceholder')}
        icon={
          <svg className="h-5 w-5 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
          </svg>
        }
      />
      
      {error && (
        <p className="mt-1 text-sm text-red-600">{error}</p>
      )}
      
      {!isLoaded && (
        <p className="mt-1 text-sm text-gray-500">{t('opportunities.loadingPlaces')}</p>
      )}
    </div>
  );
};