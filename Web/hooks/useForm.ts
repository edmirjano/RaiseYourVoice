import { useState, useCallback, ChangeEvent, FormEvent } from 'react';

type ValidationRules<T> = {
  [K in keyof T]?: {
    required?: boolean;
    minLength?: number;
    maxLength?: number;
    pattern?: RegExp;
    validate?: (value: T[K]) => boolean | string;
  };
};

type ValidationErrors<T> = {
  [K in keyof T]?: string;
};

/**
 * Custom hook for form handling with validation
 */
export function useForm<T extends Record<string, any>>(
  initialValues: T,
  validationRules?: ValidationRules<T>,
  onSubmit?: (values: T) => void | Promise<void>
) {
  const [values, setValues] = useState<T>(initialValues);
  const [errors, setErrors] = useState<ValidationErrors<T>>({});
  const [touched, setTouched] = useState<Record<keyof T, boolean>>({} as Record<keyof T, boolean>);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isValid, setIsValid] = useState(true);
  
  // Validate a single field
  const validateField = useCallback(
    (name: keyof T, value: any): string | undefined => {
      if (!validationRules || !validationRules[name]) return undefined;
      
      const rules = validationRules[name]!;
      
      if (rules.required && (!value || (typeof value === 'string' && value.trim() === ''))) {
        return 'This field is required';
      }
      
      if (rules.minLength && typeof value === 'string' && value.length < rules.minLength) {
        return `Must be at least ${rules.minLength} characters`;
      }
      
      if (rules.maxLength && typeof value === 'string' && value.length > rules.maxLength) {
        return `Must be no more than ${rules.maxLength} characters`;
      }
      
      if (rules.pattern && typeof value === 'string' && !rules.pattern.test(value)) {
        return 'Invalid format';
      }
      
      if (rules.validate) {
        const result = rules.validate(value);
        if (typeof result === 'string') {
          return result;
        }
        if (result === false) {
          return 'Invalid value';
        }
      }
      
      return undefined;
    },
    [validationRules]
  );
  
  // Validate all fields
  const validateForm = useCallback((): boolean => {
    if (!validationRules) return true;
    
    const newErrors: ValidationErrors<T> = {};
    let formIsValid = true;
    
    Object.keys(validationRules).forEach(key => {
      const fieldName = key as keyof T;
      const error = validateField(fieldName, values[fieldName]);
      
      if (error) {
        newErrors[fieldName] = error;
        formIsValid = false;
      }
    });
    
    setErrors(newErrors);
    setIsValid(formIsValid);
    
    return formIsValid;
  }, [validationRules, values, validateField]);
  
  // Handle input change
  const handleChange = useCallback(
    (e: ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
      const { name, value, type } = e.target;
      const fieldName = name as keyof T;
      
      // Handle different input types
      let fieldValue: any = value;
      if (type === 'checkbox') {
        fieldValue = (e.target as HTMLInputElement).checked;
      } else if (type === 'number') {
        fieldValue = value === '' ? '' : Number(value);
      }
      
      setValues(prev => ({ ...prev, [fieldName]: fieldValue }));
      
      // Mark field as touched
      setTouched(prev => ({ ...prev, [fieldName]: true }));
      
      // Validate field if rules exist
      if (validationRules && validationRules[fieldName]) {
        const error = validateField(fieldName, fieldValue);
        setErrors(prev => ({ ...prev, [fieldName]: error }));
        
        // Update form validity
        const newErrors = { ...errors, [fieldName]: error };
        setIsValid(!Object.values(newErrors).some(error => error !== undefined));
      }
    },
    [errors, validateField, validationRules]
  );
  
  // Set a specific field value programmatically
  const setValue = useCallback((name: keyof T, value: any) => {
    setValues(prev => ({ ...prev, [name]: value }));
    
    // Validate field if rules exist
    if (validationRules && validationRules[name]) {
      const error = validateField(name, value);
      setErrors(prev => ({ ...prev, [name]: error }));
      
      // Update form validity
      const newErrors = { ...errors, [name]: error };
      setIsValid(!Object.values(newErrors).some(error => error !== undefined));
    }
  }, [errors, validateField, validationRules]);
  
  // Handle form submission
  const handleSubmit = useCallback(
    async (e: FormEvent<HTMLFormElement>) => {
      e.preventDefault();
      
      // Mark all fields as touched
      const allTouched = Object.keys(values).reduce(
        (acc, key) => ({ ...acc, [key]: true }),
        {} as Record<keyof T, boolean>
      );
      setTouched(allTouched);
      
      // Validate all fields
      const formIsValid = validateForm();
      
      if (formIsValid && onSubmit) {
        setIsSubmitting(true);
        try {
          await onSubmit(values);
        } finally {
          setIsSubmitting(false);
        }
      }
    },
    [values, validateForm, onSubmit]
  );
  
  // Reset form to initial values
  const resetForm = useCallback(() => {
    setValues(initialValues);
    setErrors({});
    setTouched({} as Record<keyof T, boolean>);
    setIsSubmitting(false);
    setIsValid(true);
  }, [initialValues]);
  
  return {
    values,
    errors,
    touched,
    isSubmitting,
    isValid,
    handleChange,
    handleSubmit,
    setValue,
    resetForm,
    validateForm
  };
}