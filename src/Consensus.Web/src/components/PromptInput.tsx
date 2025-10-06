import { useState } from 'react';

interface PromptInputProps {
  onSubmit: (prompt: string) => void;
  disabled?: boolean;
  value?: string;
  onChange?: (value: string) => void;
}

export function PromptInput({ 
  onSubmit, 
  disabled = false,
  value: externalValue,
  onChange: externalOnChange
}: PromptInputProps) {
  const [internalPrompt, setInternalPrompt] = useState('');
  
  const prompt = externalValue !== undefined ? externalValue : internalPrompt;
  const setPrompt = externalOnChange !== undefined ? externalOnChange : setInternalPrompt;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (prompt.trim() && !disabled) {
      onSubmit(prompt);
      if (externalValue === undefined) {
        setPrompt('');
      }
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    // Only Ctrl+Enter submits the form, Enter creates newlines
    if (e.key === 'Enter' && e.ctrlKey) {
      e.preventDefault();
      handleSubmit(e);
    }
  };

  return (
    <form
      onSubmit={handleSubmit}
      className="relative w-full max-w-[700px] mx-auto"
    >
      <div className="flex items-end bg-white rounded-3xl border border-gray-300 shadow-md p-3 transition-all duration-200 focus-within:border-primary focus-within:shadow-lg focus-within:shadow-primary/15">
        <textarea
          rows={3}
          value={prompt}
          onChange={(e) => setPrompt(e.target.value)}
          onKeyDown={handleKeyDown}
          placeholder="Enter your prompt here..."
          disabled={disabled}
          className="flex-1 text-base leading-normal resize-none outline-none border-none bg-transparent disabled:opacity-50"
          style={{ maxHeight: '200px' }}
        />
        <button
          type="submit"
          disabled={!prompt.trim() || disabled}
          className={`ml-2 p-2 w-9 h-9 rounded-full transition-colors duration-200 font-sans ${
            prompt.trim() && !disabled
              ? '!bg-blue-600 hover:!bg-blue-700 !text-white cursor-pointer'
              : '!bg-gray-300 !text-gray-500 cursor-not-allowed pointer-events-none'
          }`}
        >
          <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
            <path d="M4 12l1.41 1.41L11 7.83V20h2V7.83l5.58 5.59L20 12l-8-8-8 8z" />
          </svg>
        </button>
      </div>
    </form>
  );
}
