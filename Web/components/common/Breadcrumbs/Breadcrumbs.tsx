import React from 'react';
import Link from 'next/link';

type BreadcrumbItem = {
  label: string;
  href?: string;
};

type BreadcrumbsProps = {
  items: BreadcrumbItem[];
  className?: string;
  separator?: React.ReactNode;
};

export const Breadcrumbs: React.FC<BreadcrumbsProps> = ({
  items,
  className = '',
  separator = (
    <svg
      className="h-4 w-4 text-gray-400"
      fill="none"
      viewBox="0 0 24 24"
      stroke="currentColor"
    >
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        strokeWidth={2}
        d="M9 5l7 7-7 7"
      />
    </svg>
  ),
}) => {
  if (!items.length) return null;

  return (
    <nav className={`flex ${className}`} aria-label="Breadcrumb">
      <ol className="flex items-center space-x-2">
        {items.map((item, index) => (
          <li key={index} className="flex items-center">
            {index > 0 && <span className="mx-2">{separator}</span>}
            {index === items.length - 1 || !item.href ? (
              <span className="text-sm font-medium text-gray-500">{item.label}</span>
            ) : (
              <Link
                href={item.href}
                className="text-sm font-medium text-ios-black hover:underline"
              >
                {item.label}
              </Link>
            )}
          </li>
        ))}
      </ol>
    </nav>
  );
};