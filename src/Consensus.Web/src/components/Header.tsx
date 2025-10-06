interface HeaderProps {
  align?: 'left' | 'center';
  noMargin?: boolean;
}

export function Header({ align = 'left', noMargin = false }: HeaderProps) {
  return (
    <div className={`${noMargin ? '' : 'mb-12'} text-${align}`}>
      <h1 
        className={`text-3xl font-extralight font-sans ${noMargin ? 'm-0 leading-none' : ''}`}
      >
        consensus
      </h1>
    </div>
  );
}
