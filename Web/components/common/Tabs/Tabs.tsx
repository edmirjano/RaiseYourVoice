import React, { useState } from 'react';
import { motion } from 'framer-motion';

type Tab = {
  id: string;
  label: React.ReactNode;
  content: React.ReactNode;
  disabled?: boolean;
  badge?: React.ReactNode;
};

type TabsProps = {
  tabs: Tab[];
  defaultTab?: string;
  onChange?: (tabId: string) => void;
  className?: string;
  variant?: 'underline' | 'pills' | 'buttons';
};

export const Tabs: React.FC<TabsProps> = ({
  tabs,
  defaultTab,
  onChange,
  className = '',
  variant = 'underline',
}) => {
  const [activeTab, setActiveTab] = useState(defaultTab || tabs[0]?.id || '');

  const handleTabChange = (tabId: string) => {
    setActiveTab(tabId);
    if (onChange) {
      onChange(tabId);
    }
  };

  const getTabStyles = (tab: Tab) => {
    const isActive = activeTab === tab.id;
    const isDisabled = tab.disabled;

    if (variant === 'underline') {
      return {
        tab: `px-4 py-3 text-sm font-medium ${
          isActive
            ? 'border-b-2 border-ios-black text-ios-black'
            : isDisabled
            ? 'text-gray-400 cursor-not-allowed'
            : 'text-gray-500 hover:text-gray-700 hover:border-gray-300'
        }`,
        container: 'border-b border-gray-200',
      };
    }

    if (variant === 'pills') {
      return {
        tab: `px-4 py-2 text-sm font-medium rounded-full ${
          isActive
            ? 'bg-ios-black text-white'
            : isDisabled
            ? 'bg-gray-100 text-gray-400 cursor-not-allowed'
            : 'bg-gray-100 text-gray-800 hover:bg-gray-200'
        }`,
        container: 'space-x-2',
      };
    }

    // Buttons variant
    return {
      tab: `px-4 py-2 text-sm font-medium rounded-md ${
        isActive
          ? 'bg-ios-black text-white'
          : isDisabled
          ? 'bg-gray-100 text-gray-400 cursor-not-allowed'
          : 'bg-white border border-gray-300 text-gray-700 hover:bg-gray-50'
      }`,
      container: 'space-x-2',
    };
  };

  return (
    <div className={className}>
      <div className={getTabStyles(tabs[0]).container}>
        <nav className="flex">
          {tabs.map((tab) => (
            <button
              key={tab.id}
              onClick={() => !tab.disabled && handleTabChange(tab.id)}
              className={getTabStyles(tab).tab}
              disabled={tab.disabled}
            >
              <div className="flex items-center">
                {tab.label}
                {tab.badge && <span className="ml-2">{tab.badge}</span>}
              </div>
            </button>
          ))}
        </nav>
      </div>

      <div className="mt-4">
        {tabs.map((tab) => (
          <div
            key={tab.id}
            className={activeTab === tab.id ? 'block' : 'hidden'}
          >
            {activeTab === tab.id && (
              <motion.div
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                transition={{ duration: 0.2 }}
              >
                {tab.content}
              </motion.div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
};