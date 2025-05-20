import React, { forwardRef } from 'react';

type InputProps = {
  type?: 'text' | 'email' | 'password' | 'number' | 'tel' | 'url' | 'date';
  placeholder?: string;
  value?: string | number;
  onChange?: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onBlur?: (e: React.FocusEvent<HTMLInputElement>) => void;
  disabled?: boolean;
  required?: boolean;
  className?: string;
  error?: boolean;
  id?: string;
  name?: string;
  autoComplete?: string;
  min?: number | string;
  max?: number | string;
  step?: number | string;
  pattern?: string;
  readOnly?: boolean;
  icon?: React.ReactNode;
};

export const Input = forwardRef<HTMLInputElement, InputProps>(
  (
    {
      type = 'text',
      placeholder,
      value,
      onChange,
      onBlur,
      disabled = false,
      required = false,
      className = '',
      error = false,
      id,
      name,
      autoComplete,
      min,
      max,
      step,
      pattern,
      readOnly = false,
      icon,
    },
    ref
  ) => {
    const baseClasses = 'ios-input w-full';
    const errorClasses = error ? 'border-red-500 focus:border-red-500 focus:ring-red-500' : '';
    const disabledClasses = disabled ? 'bg-gray-100 cursor-not-allowed' : '';
    const iconClasses = icon ? 'pl-10' : '';

    return (
      <div className="relative">
        {icon && (
          <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
            {icon}
          </div>
        )}
        <input
          ref={ref}
          type={type}
          placeholder={placeholder}
          value={value}
          onChange={onChange}
          onBlur={onBlur}
          disabled={disabled}
          required={required}
          className={`${baseClasses} ${errorClasses} ${disabledClasses} ${iconClasses} ${className}`}
          id={id}
          name={name}
          autoComplete={autoComplete}
          min={min}
          max={max}
          step={step}
          pattern={pattern}
          readOnly={readOnly}
        />
      </div>
    );
  }
);

Input.displayName = 'Input';