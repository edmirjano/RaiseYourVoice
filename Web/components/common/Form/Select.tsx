import React, { forwardRef } from 'react';

type SelectOption = {
  value: string;
  label: string;
};

type SelectProps = {
  options: SelectOption[];
  value?: string;
  onChange?: (e: React.ChangeEvent<HTMLSelectElement>) => void;
  onBlur?: (e: React.FocusEvent<HTMLSelectElement>) => void;
  disabled?: boolean;
  required?: boolean;
  className?: string;
  error?: boolean;
  id?: string;
  name?: string;
  placeholder?: string;
};

export const Select = forwardRef<HTMLSelectElement, SelectProps>(
  (
    {
      options,
      value,
      onChange,
      onBlur,
      disabled = false,
      required = false,
      className = '',
      error = false,
      id,
      name,
      placeholder,
    },
    ref
  ) => {
    const baseClasses = 'ios-input w-full';
    const errorClasses = error ? 'border-red-500 focus:border-red-500 focus:ring-red-500' : '';
    const disabledClasses = disabled ? 'bg-gray-100 cursor-not-allowed' : '';

    return (
      <select
        ref={ref}
        value={value}
        onChange={onChange}
        onBlur={onBlur}
        disabled={disabled}
        required={required}
        className={`${baseClasses} ${errorClasses} ${disabledClasses} ${className}`}
        id={id}
        name={name}
      >
        {placeholder && (
          <option value="" disabled>
            {placeholder}
          </option>
        )}
        {options.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
    );
  }
);

Select.displayName = 'Select';