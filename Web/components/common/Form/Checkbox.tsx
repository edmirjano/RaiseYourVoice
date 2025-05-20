import React, { forwardRef } from 'react';

type CheckboxProps = {
  label?: React.ReactNode;
  checked?: boolean;
  onChange?: (e: React.ChangeEvent<HTMLInputElement>) => void;
  disabled?: boolean;
  required?: boolean;
  className?: string;
  id?: string;
  name?: string;
  value?: string;
  helpText?: string;
};

export const Checkbox = forwardRef<HTMLInputElement, CheckboxProps>(
  (
    {
      label,
      checked,
      onChange,
      disabled = false,
      required = false,
      className = '',
      id,
      name,
      value,
      helpText,
    },
    ref
  ) => {
    return (
      <div className={`flex items-start ${className}`}>
        <div className="flex items-center h-5">
          <input
            ref={ref}
            type="checkbox"
            checked={checked}
            onChange={onChange}
            disabled={disabled}
            required={required}
            className="h-4 w-4 text-ios-black focus:ring-ios-black border-gray-300 rounded"
            id={id}
            name={name}
            value={value}
          />
        </div>
        {(label || helpText) && (
          <div className="ml-3 text-sm">
            {label && (
              <label
                htmlFor={id}
                className={`font-medium ${disabled ? 'text-gray-400' : 'text-gray-700'}`}
              >
                {label}
                {required && <span className="text-red-500 ml-1">*</span>}
              </label>
            )}
            {helpText && <p className="text-gray-500">{helpText}</p>}
          </div>
        )}
      </div>
    );
  }
);

Checkbox.displayName = 'Checkbox';