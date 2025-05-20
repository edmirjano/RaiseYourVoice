import React, { forwardRef } from 'react';

type TextAreaProps = {
  placeholder?: string;
  value?: string;
  onChange?: (e: React.ChangeEvent<HTMLTextAreaElement>) => void;
  onBlur?: (e: React.FocusEvent<HTMLTextAreaElement>) => void;
  disabled?: boolean;
  required?: boolean;
  className?: string;
  error?: boolean;
  id?: string;
  name?: string;
  rows?: number;
  maxLength?: number;
  readOnly?: boolean;
};

export const TextArea = forwardRef<HTMLTextAreaElement, TextAreaProps>(
  (
    {
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
      rows = 4,
      maxLength,
      readOnly = false,
    },
    ref
  ) => {
    const baseClasses = 'ios-input w-full';
    const errorClasses = error ? 'border-red-500 focus:border-red-500 focus:ring-red-500' : '';
    const disabledClasses = disabled ? 'bg-gray-100 cursor-not-allowed' : '';

    return (
      <textarea
        ref={ref}
        placeholder={placeholder}
        value={value}
        onChange={onChange}
        onBlur={onBlur}
        disabled={disabled}
        required={required}
        className={`${baseClasses} ${errorClasses} ${disabledClasses} ${className}`}
        id={id}
        name={name}
        rows={rows}
        maxLength={maxLength}
        readOnly={readOnly}
      />
    );
  }
);

TextArea.displayName = 'TextArea';