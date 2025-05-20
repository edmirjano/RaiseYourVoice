import React, { forwardRef } from 'react';

type RadioOption = {
  value: string;
  label: string;
  disabled?: boolean;
};

type RadioGroupProps = {
  options: RadioOption[];
  value?: string;
  onChange?: (e: React.ChangeEvent<HTMLInputElement>) => void;
  name: string;
  className?: string;
  inline?: boolean;
};

export const RadioGroup = forwardRef<HTMLDivElement, RadioGroupProps>(
  ({ options, value, onChange, name, className = '', inline = false }, ref) => {
    return (
      <div
        ref={ref}
        className={`${inline ? 'flex space-x-4' : 'space-y-2'} ${className}`}
      >
        {options.map((option) => (
          <div key={option.value} className="flex items-center">
            <input
              id={`${name}-${option.value}`}
              name={name}
              type="radio"
              value={option.value}
              checked={value === option.value}
              onChange={onChange}
              disabled={option.disabled}
              className="h-4 w-4 text-ios-black focus:ring-ios-black border-gray-300"
            />
            <label
              htmlFor={`${name}-${option.value}`}
              className={`ml-2 block text-sm font-medium ${
                option.disabled ? 'text-gray-400' : 'text-gray-700'
              }`}
            >
              {option.label}
            </label>
          </div>
        ))}
      </div>
    );
  }
);

RadioGroup.displayName = 'RadioGroup';