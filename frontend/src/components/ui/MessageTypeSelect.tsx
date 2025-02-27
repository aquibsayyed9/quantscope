import { useState } from "react";
import { MessageTypes } from "../../constants/fix-message-types";
import { Check } from "lucide-react";

export const MessageTypeSelect = ({ 
    selected, 
    onChange 
  }: { 
    selected: string[],
    onChange: (types: string[]) => void 
  }) => {
    const [isOpen, setIsOpen] = useState(false);
  
    const messageTypeOptions = Object.entries(MessageTypes).map(([value, info]) => ({
      value,
      label: info.label,
      color: info.color
    }));
  
    const toggleOption = (value: string) => {
      const newSelected = selected.includes(value)
        ? selected.filter(type => type !== value)
        : [...selected, value];
      onChange(newSelected);
    };
  
    return (
      <div className="relative">
        <div
          className="w-full px-3 py-2 border rounded-md cursor-pointer bg-white"
          onClick={() => setIsOpen(!isOpen)}
        >
          {selected.length === 0 ? (
            <span className="text-gray-500">Select message types...</span>
          ) : (
            <div className="flex flex-wrap gap-1">
              {selected.map(type => (
                <span
                  key={type}
                  className={`${MessageTypes[type]?.color || 'bg-gray-100'} px-2 py-1 rounded text-sm`}
                >
                  {MessageTypes[type]?.label || type}
                </span>
              ))}
            </div>
          )}
        </div>
        
        {isOpen && (
          <div className="absolute z-10 w-full mt-1 bg-white border rounded-md shadow-lg max-h-60 overflow-auto">
            {messageTypeOptions.map(({ value, label, color }) => (
              <div
                key={value}
                className="px-3 py-2 hover:bg-gray-50 cursor-pointer flex items-center space-x-2"
                onClick={() => toggleOption(value)}
              >
                <div className="w-4 h-4 border rounded flex items-center justify-center">
                  {selected.includes(value) && <Check className="w-3 h-3" />}
                </div>
                <span className={`${color} px-2 py-1 rounded text-sm flex-grow`}>
                  {label}
                </span>
              </div>
            ))}
          </div>
        )}
      </div>
    );
  };