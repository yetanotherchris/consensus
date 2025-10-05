declare module 'react-lazylog' {
  import { CSSProperties } from 'react';

  export interface LazyLogProps {
    text?: string;
    url?: string;
    enableSearch?: boolean;
    follow?: boolean;
    selectableLines?: boolean;
    extraLines?: number;
    caseInsensitive?: boolean;
    style?: CSSProperties;
    containerStyle?: CSSProperties;
    height?: string | number;
    width?: string | number;
    lineClassName?: string;
    highlightLineClassName?: string;
    onError?: (error: Error) => void;
    onLoad?: () => void;
    onHighlight?: (range: { first: number; last: number }) => void;
    scrollToLine?: number;
    rowHeight?: number;
    overscanRowCount?: number;
  }

  export class LazyLog extends React.Component<LazyLogProps> {}
}
